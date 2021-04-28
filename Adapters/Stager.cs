using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
        private const byte indexingFix = 0; //To comply with 0 indexing
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
                if (stage.Text != null) ui.ChangeText(stage.Text);
                if (stage.Image != null) ui.ChangeImage(stage.Image);
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
                    if (actionsWords.Length < 4) 
                        throw new ArgumentException($"Not enough parameters for {actionName} action. StageIndex:{previousStageIndex}");
                    
                    if (Int32.TryParse(actionsWords[1], out var fightPlanIndex) &&
                        fightPlanIndex < game.fightPlans.Count)
                    {
                        Fighter fighter = new Fighter(this, game.fightPlans[fightPlanIndex]);
                        ui.StartFight();
                        ui.ChangeAdapter(fighter);
                    }
                    else throw new ArgumentException($"There is no fight plan with specified index - {actionsWords[1]}" +
                                                     $"Stage: {previousStageIndex}");
                    

                    if (Int32.TryParse(actionsWords[2], out fightWinStage) &&
                        Int32.TryParse(actionsWords[3], out fightRunStage))
                    {
                        fightRunStage -= indexingFix;
                        fightWinStage -= indexingFix;
                        if (fightWinStage >= stages.Count || fightRunStage >= stages.Count)
                        {
                            throw new ArgumentException(
                                $"There is no stage with one or both specified IDs - {actionsWords[2]}, {actionsWords[3]}. " +
                                $"StageIndex:{previousStageIndex}");
                        }
                    }
                    else
                        throw new ArgumentException(
                            $"One or both stage IDs are incorrect (not integers) - {actionsWords[2]}, {actionsWords[3]}. " +
                            $"StageIndex:{previousStageIndex}");
                    
                    break;
                }
                case "set":
                {
                    if (actionsWords.Length < 4) 
                        throw new ArgumentException($"Not enough parameters for {actionName} action. StageIndex:{previousStageIndex}");
                    
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
                    try
                    {
                        switch (actionsWords[2])
                        {
                            case "bool":
                            {
                                var value = Boolean.Parse(stringValue);
                                property.SetValue(game.hero, value);
                                break;
                            }
                            case "string":
                            {
                                property.SetValue(game.hero, stringValue);
                                break;
                            }
                            case "int":
                            {
                                var value = Int32.Parse(stringValue);
                                property.SetValue(game.hero, value);
                                break;
                            }
                            case "Race":
                            {
                                var value = Enum.Parse<Race>(stringValue);
                                property.SetValue(game.hero, value);
                                break;
                            }
                            case "Sex":
                            {
                                var value = Enum.Parse<Sex>(stringValue);
                                property.SetValue(game.hero, value);
                                break;
                            }
                            default:
                                throw new ArgumentException(
                                    $"Specified property type is not supported - {actionsWords[2]}" +
                                    $"StageIndex:{previousStageIndex}");
                        }
                    }
                    catch (ArgumentNullException ex)
                    {
                        throw new ArgumentException($"Specified value is null" +
                                                    $"StageIndex:{previousStageIndex}");
                    }
                    catch (Exception ex) when (ex is TargetInvocationException || ex is FormatException)
                    {
                        if (actionsWords[3].Equals(textboxKeyword))
                        {
                            ChangeStage(previousStageIndex);
                            MessageBox.Show("Некорректный ввод, попробуйте снова.");
                        }
                        else
                        {
                            throw new ArgumentException($"Specified value is incorrect - {actionsWords[3]}" +
                                                        $"StageIndex:{previousStageIndex}");
                        }
                    }

                    break;
                }
                case "get":
                {
                    if (actionsWords.Length < 2) 
                        throw new ArgumentException($"Not enough parameters for {actionName} action. StageIndex:{previousStageIndex}");
                    
                    object createdObject = GetObjectFromString(actionsWords[1], SubArray(actionsWords, 2, actionsWords.Length - 2));

                    if (createdObject is Artifact)
                    {
                        game.hero.PickUpArtifact(createdObject as Artifact);
                    }

                    break;
                }
                case "learn":
                {
                    if (actionsWords.Length < 2) 
                        throw new ArgumentException($"Not enough parameters for {actionName} action. StageIndex:{previousStageIndex}");

                    object createdObject = GetObjectFromString(actionsWords[1], SubArray(actionsWords, 2, actionsWords.Length - 2));

                    if (createdObject is Spell)
                    {
                        game.hero.LearnSpell(createdObject as Spell);
                    }
                    
                    break;
                }
                case "info":
                case "repeat": //метод о выписывании инфы по персонажу выписывает инфу в окно
                { 
                    ui.ChangeText(ui.MainText.Text + "\n" + game.hero.ToString());
                    
                    break; 
                }
                case "damage": // наносит урон игроку
                {
                    if (actionsWords.Length < 2) 
                        throw new ArgumentException($"Not enough parameters for {actionName} action. StageIndex:{previousStageIndex}");
                    
                    if (Int32.TryParse(actionsWords[1], out var amountDamage))
                    {
                        game.hero.Health -= amountDamage;
                    }
                    
                    break; 
                }
                case "getexp": //дать игроку опыт
                {
                    if (actionsWords.Length < 2) 
                        throw new ArgumentException($"Not enough parameters for {actionName} action. StageIndex:{previousStageIndex}");
                    
                    if (Int32.TryParse(actionsWords[1], out var amountExp))
                    {
                        game.hero.Experience += amountExp;
                    }
                    
                    break; 
                }
                case "camp": break; // то самое окно, где можно учить спеллы и открыть инвентарь
                case "compexp": //сравнивает экспу игрока и гнома 
                {
                    //Make compare with number supplied by action?
                    if (actionsWords.Length < 3) 
                        throw new ArgumentException($"Not enough parameters for {actionName} action. StageIndex:{previousStageIndex}");
                    
                    string stageIndexString = game.hero.CompareTo(/* change */ game.Gnom) >= 0 ? actionsWords[1] : actionsWords[2];

                    if (Int32.TryParse(stageIndexString, out var stageIndex))
                    {
                        stageIndex -= indexingFix;
                        ChangeStage(stageIndex);    
                    }
                    
                    break; 
                }
                case "end":  //конец игры
                {
                    EndGame();
                    
                    break;
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
            
            public string Image { get; set; }
            public List<int> Next { get; set; }
        }
        
    }
}