using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using KashTaskWPF.Artifacts;
using KashTaskWPF.Adapters;
using game;
using KashTaskWPF.Spells;

namespace KashTaskWPF.Adapters
{
    public class Stager : IAdapter
    {
        private List<Stage> stages;
        private int currentStageIndex;
        private int previousStageIndex;
        
        private int fightWinStage;
        private int fightRunStage;

        private string lastUserInput;

        public Game game;
        private MainWindow ui;
        private const string FILENAME = @"Resources/game.json";
        private const byte indexingFix = 1; //To comply with 0 indexing
        private const string textboxKeyword = "<TEXTBOX>"; //Magic word from json for displaying textbox instead of buttons
        public Stager(MainWindow window)
        {
            ui = window;
            stages = JsonInit(FILENAME);

            currentStageIndex = 0;
            DisplayStage(GetCurrentStage());
            game = new Game();
        }

        public void GetInput(int index)
        {
            HandleAnswer(index);
        }

        public void HandleAnswer(int answerIndex)
        {
            Stage currentStage = GetCurrentStage();

            if (currentStage != null && currentStage.Next.Count > answerIndex)
            {
                previousStageIndex = currentStageIndex;
                lastUserInput = ui.GetUserInputText();
                ChangeStage(currentStage.Next[answerIndex] - indexingFix);
                
                if (currentStage.Actions.ContainsKey(answerIndex.ToString()))
                {
                    foreach (var action in currentStage.Actions[answerIndex.ToString()])
                    {
                        DoAction(action);
                        Console.WriteLine("Action");
                    }
                }
            }
        }

        public void ChangeStage(int stageIndex)
        {
            if (stageIndex < stages.Count)
            {
                currentStageIndex = stageIndex;
                DisplayStage(GetCurrentStage());
            }
        }

        private void DisplayStage(Stage stage)
        {
            if (stage != null)
            {
                ui.ChangeText(stage.Text);
                if (stage.Answers.Count >= 1 && stage.Answers[0].Equals(textboxKeyword))
                {
                    ui.ChangeNumberOfButtons(1);
                    ui.ChangeButtonsText(new List<string> {"Далее"});
                    ui.DisplayTextBox();
                }
                else if (stage.Answers.Count == 0)
                {
                    ui.ChangeNumberOfButtons(1);
                    ui.ChangeButtonsText(new List<string> {"Далее"});
                    ui.HideTextBox();
                }
                else
                {
                    ui.ChangeNumberOfButtons(stage.Next.Count);
                    ui.ChangeButtonsText(stage.Answers);
                    ui.HideTextBox();
                }
            }
        }

        private void DoAction(string actionName)
        {
            actionName = actionName.Trim();
            var actionsWords = actionName.Split(' ');
            string command = actionsWords[0].ToLower();
            Console.WriteLine(command);
            switch (command)
            {
                case "fight":
                {                  
                        ui.StartFight();
                        List<FightPlan> fightplans = game.fightPlans;
                        Fighter fighter = new Fighter(this, fightplans[0]);
                        fightplans.RemoveAt(0);

                        if (Int32.TryParse(actionsWords[1], out fightWinStage) && Int32.TryParse(actionsWords[2], out fightRunStage))
                        {
                            fightRunStage -= indexingFix; fightWinStage -= indexingFix;
                            if (fightWinStage >= stages.Count || fightRunStage >= stages.Count)
                            {
                                throw new ArgumentException($"There is no stage with one or both specified IDs - {actionsWords[1]}, {actionsWords[2]}. " +
                                                            $"StageIndex:{previousStageIndex}");
                            }
                        }
                        else throw new ArgumentException($"One or both stage IDs are incorrect (not integers) - {actionsWords[1]}, {actionsWords[2]}. " +
                                                         $"StageIndex:{previousStageIndex}");
                        ui.ChangeAdapter(fighter);
                        break;
                }
                case "set":
                {
                    var property = game.hero.GetType().GetProperty(actionsWords[1]);
                    if (property == null)
                    {
                        throw new ArgumentException($"There is no property with specified name - {actionsWords[1]}" +
                                                    $"StageIndex:{previousStageIndex}");
                    }
                    if (!property.CanWrite)
                    {
                        throw new ArgumentException($"Specified property is readonly - {actionsWords[1]}" +
                                                    $"StageIndex:{previousStageIndex}");
                    }

                    string stringValue = actionsWords[3].Equals("<TEXTBOX>") ? lastUserInput : actionsWords[3];
                    switch (actionsWords[2])
                    {
                        case "bool":
                        {
                            if (Boolean.TryParse(stringValue, out bool value))
                            {
                                property.SetValue(game.hero, value);
                            }
                            break;
                        }
                        case "string":
                        {
                            property.SetValue(game.hero, stringValue);
                            break;
                        }
                        case "int":
                        {
                            if (Int32.TryParse(stringValue, out int value))
                            {
                                property.SetValue(game.hero, value);
                            }
                            break;
                        }
                        case "Race":
                        {
                            if (Enum.TryParse<Race>(stringValue, out var value))
                            {
                                property.SetValue(game.hero, value);
                            }
                            break;
                        }
                        case "Sex":
                        {
                            if (Enum.TryParse<Sex>(stringValue, out var value))
                            {
                                property.SetValue(game.hero, value);
                            }
                            break;
                        }
                        default: 
                            throw new ArgumentException($"Specified property type is not supported - {actionsWords[2]}" +
                                                        $"StageIndex:{previousStageIndex}");
                    }
                    break;
                }
                case "get":
                {
                    object createdObject = GetObjectFromString(actionsWords[1], SubArray(actionsWords, 2, actionsWords.Length - 2));

                    if (createdObject is Artifact)
                    {
                        game.hero.PickUpArtifact(createdObject as Artifact);
                    }

                    break;
                }
                case "learn":
                {
                    object createdObject = GetObjectFromString(actionsWords[1], SubArray(actionsWords, 2, actionsWords.Length - 2));

                    if (createdObject is Spell)
                    {
                        game.hero.LearnSpell(createdObject as Spell);
                    }
                    
                    break;
                }
                case "info":
                case "repeat": {
                    ui.ChangeText(ui.MainText.Text + "\n" + game.hero.ToString());
                    
                    break; //метод о выписывании инфы по персонажу выписывает инфу в окно
                }
                case "damage":
                {
                    if (Int32.TryParse(actionsWords[1], out var amountDamage))
                    {
                        game.hero.Health -= amountDamage;
                    }
                    
                    break; // наносит урон игроку
                }
                case "getexp": 
                {
                    if (Int32.TryParse(actionsWords[1], out var amountExp))
                    {
                        game.hero.Experience += amountExp;
                    }
                    
                    break; //дать игроку опыт
                }
                case "camp": break; // то самое окно, где можно учить спеллы и открыть инвентарь
                case "compexp": //Make compare with number in action?
                {
                    string stageIndexString = game.hero.CompareTo(/* change */ game.Gnom) >= 0 ? actionsWords[1] : actionsWords[2];

                    if (Int32.TryParse(stageIndexString, out var stageIndex))
                    {
                        stageIndex -= indexingFix;
                        ChangeStage(stageIndex);    
                    }
                    
                    break; //сравнивает экспу игрока и гнома
                }
                case "end": 
                {
                    EndGame();
                    
                    break; //конец игры
                }
                default:
                    throw new ArgumentException($"There is no action with specified name - {actionName}. " +
                                                $"StageIndex:{previousStageIndex}");
            }
        }

        private object GetObjectFromString(string className, string[] parameters)
        {
            Type type = Type.GetType(className);
            if (type == null)
            {
                throw new ArgumentException($"Type with specified name wasn't found - {className}. " +
                                                 $"StageIndex:{previousStageIndex}");
            }
                    
            object createdObject = null;
            try
            {
                if (parameters.Length > 0)
                {
                    createdObject = Activator.CreateInstance(type, new object[] {Int32.Parse(parameters[0])});
                }
                else
                {
                    createdObject = Activator.CreateInstance(type);
                }
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Cannot construct specified type - {className}. " +
                                            $"Parameters: {parameters}. " +
                                            $"StageIndex: {previousStageIndex}");
            }

            return createdObject;
        }

        public void EndFight(FightResult result)
        {
            switch (result)
            {
                case FightResult.WON:
                {
                    MessageBox.Show("Об этой битве будут слагать легенды!");
                    ChangeStage(fightWinStage);
                    break;
                }
                case FightResult.DIED:
                {
                    MessageBox.Show("Вы погибли... Не расстраивайтесь, попробуйте снова :)");
                    ChangeStage(previousStageIndex);
                    break;
                }
                case FightResult.RAN:
                {
                    MessageBox.Show("Иногда лучше отступить, чтобы напасть вновь.");
                    ChangeStage(fightRunStage);
                    break;
                }
            }
            
            ui.EndFight(result);
            ui.ChangeAdapter(this);

        }

        private void EndGame() //TODO:
        {
            
        }

        public Stage GetCurrentStage()
        {
            if (currentStageIndex < stages.Count)
            {
                return stages[currentStageIndex];
            }

            return null;
        }
            
        private List<Stage> JsonInit(string filename)
        {
            var reader = new StreamReader(filename);
            var jsonString = reader.ReadToEnd();
            reader.Close();
            var deserialize = JsonSerializer.Deserialize<List<Stage>>(jsonString);

            return deserialize;
        }
        
        private static T[] SubArray<T>(T[] array, int offset, int length)
        {
            T[] result = new T[length];
            Array.Copy(array, offset, result, 0, length);
            return result;
        }
        
        public class Stage
        {
            public Stage()
            {
            }
        
            public int ID { get; set; }
            public string Text { get; set; }
            public List<string> Answers { get; set; }
            public Dictionary<string, List<string>> Actions { get; set; }
            public List<int> Next { get; set; }
        }
        
    }
}