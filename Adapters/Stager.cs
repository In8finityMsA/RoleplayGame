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
        
        private int fightWonStage;
        private int fightRanStage;
        private int fightNegotiatedStage;

        private string lastUserTextInput;

        public Game game;
        private MainWindow ui;
        private const string FILENAME = @"Resources/game.json";
        private const byte indexingFix = 0; //To comply with 0 indexing
        private const string TEXTBOX_KEYWORD = "<TEXTBOX>"; //Magic word from json for displaying textbox instead of buttons

        //Fight result messages
        private const string WON_MESSAGE = "Об этой битве будут слагать легенды!";
        private const string RAN_MESSAGE = "Иногда лучше отступить, чтобы напасть вновь.";
        private const string DIED_MESSAGE = "Вы погибли... Не расстраивайтесь, попробуйте снова :)";
        private const string NEGOTIATED_MESSAGE = "Зачем махать кулаками, если можно поговорить.";

        private const string NEXT_MESSAGE = "Далее";
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
                lastUserTextInput = ui.GetUserInputText();
                ChangeStage(currentStage.Next[answerIndex] - indexingFix);
                
                if (currentStage.Actions.ContainsKey(answerIndex.ToString()))
                {
                    foreach (var action in currentStage.Actions[answerIndex.ToString()])
                    {
                        DoAction(action);
                    }
                }
            }
        }

        private void ChangeStage(int stageIndex)
        {
            if (stageIndex < stages.Count)
            {
                currentStageIndex = stageIndex;
                DisplayStage(GetCurrentStage());
            }
            else
            {
                throw new ArgumentException($"Stage index is out of range - {stageIndex}. Stages count: {stages.Count}.");
            }
        }

        private void DisplayStage(Stage stage)
        {
            if (stage != null)
            {
                if (stage.Text != null) ui.ChangeText(stage.Text);
                if (stage.Image != null) ui.ChangeImage("Resources\\" + stage.Image);
                if (stage.Answers.Count >= 1 && stage.Answers[0].Equals(TEXTBOX_KEYWORD))
                {
                    ui.ChangeNumberOfButtons(1);
                    ui.ChangeButtonsText(new List<string> {NEXT_MESSAGE});
                    ui.DisplayTextBox();
                }
                else if (stage.Answers.Count == 0)
                {
                    ui.ChangeNumberOfButtons(1);
                    ui.ChangeButtonsText(new List<string> {NEXT_MESSAGE});
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
                case "fight": //fight <fightplan index> <image> <win stage id> <lose stage id> [negotiate stage id]
                {          
                    if (actionsWords.Length < 6) 
                        throw new ArgumentException($"Not enough parameters for {actionName} action. StageIndex:{previousStageIndex}");
                    
                    if (Int32.TryParse(actionsWords[1], out var fightPlanIndex) &&
                        fightPlanIndex < game.fightPlans.Count)
                    {
                        Fighter fighter = new Fighter(this, game.fightPlans[fightPlanIndex]);
                        ui.StartFight();
                        ui.ChangeAdapter(fighter);
                        ui.ChangeImage("Resources\\" + actionsWords[2]);
                    }
                    else throw new ArgumentException($"There is no fight plan with specified index - {actionsWords[1]}" +
                                                     $"Stage: {previousStageIndex}");
                    
                    if (Int32.TryParse(actionsWords[3], out fightWonStage) &&
                        Int32.TryParse(actionsWords[4], out fightRanStage) &&
                        Int32.TryParse(actionsWords[5], out fightNegotiatedStage))
                    {
                        fightRanStage -= indexingFix;
                        fightWonStage -= indexingFix;
                        fightNegotiatedStage -= indexingFix;
                        if (fightWonStage >= stages.Count || fightRanStage >= stages.Count || fightNegotiatedStage >= stages.Count)
                        {
                            throw new ArgumentException(
                                $"There is no stage with one or both specified IDs - {actionsWords[3]}, {actionsWords[4]}, {actionsWords[5]}. " +
                                $"StageIndex:{previousStageIndex}");
                        }
                    }
                    else
                        throw new ArgumentException(
                            $"One or both stage IDs are incorrect (not integers) - {actionsWords[3]}, {actionsWords[4]}, {actionsWords[5]}. " +
                            $"StageIndex:{previousStageIndex}");
                    
                    break;
                }
                case "set": //set <property name> <property type> <value>
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

                    string stringValue = actionsWords[3].Equals("<TEXTBOX>") ? lastUserTextInput : actionsWords[3];
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
                        if (actionsWords[3].Equals(TEXTBOX_KEYWORD))
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
                case "get": //get <artifact class name>
                {
                    if (actionsWords.Length < 2) 
                        throw new ArgumentException($"Not enough parameters for {actionName} action. StageIndex:{previousStageIndex}");
                    
                    object createdObject = GetObjectFromString("KashTaskWPF.Artifacts." + actionsWords[1], SubArray(actionsWords, 2, actionsWords.Length - 2));

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

                    object createdObject = GetObjectFromString("KashTaskWPF.Spells." + actionsWords[1], SubArray(actionsWords, 2, actionsWords.Length - 2));

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
                case "damage": // damage <value>  наносит урон игроку
                {
                    if (actionsWords.Length < 2) 
                        throw new ArgumentException($"Not enough parameters for {actionName} action. StageIndex:{previousStageIndex}");
                    
                    if (Int32.TryParse(actionsWords[1], out var amountDamage))
                    {
                        game.hero.Health -= amountDamage;
                    }
                    
                    break; 
                }
                case "getexp": // getexp <value>  дать игроку опыт
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
                case "compexp": // compexp <win stage id> <lose stage id>  сравнивает экспу игрока и гнома 
                {
                    //Make compare with number supplied by action?
                    if (actionsWords.Length < 3) 
                        throw new ArgumentException($"Not enough parameters for {actionName} action. StageIndex:{previousStageIndex}");
                    
                    string stageIndexString = game.hero.CompareTo(game.Gnom) >= 0 ? actionsWords[1] : actionsWords[2];

                    if (Int32.TryParse(stageIndexString, out var stageIndex))
                    {
                        stageIndex -= indexingFix;
                        ChangeStage(stageIndex);    
                    }
                    
                    break; 
                }
                case "end": //конец игры
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
            int stageIndex = 0;
            switch (result)
            {
                case FightResult.WON:
                {
                    MessageBox.Show(WON_MESSAGE);
                    stageIndex = fightWonStage;
                    break;
                }
                case FightResult.DIED:
                {
                    MessageBox.Show(DIED_MESSAGE);
                    stageIndex = previousStageIndex;
                    break;
                }
                case FightResult.RAN:
                {
                    MessageBox.Show(RAN_MESSAGE);
                    stageIndex = fightRanStage;
                    break;
                }
                case FightResult.NEGOTIATED:
                {
                    MessageBox.Show(NEGOTIATED_MESSAGE);
                    stageIndex = fightNegotiatedStage;
                    break;
                }
                default:
                {
                    throw new ArgumentException($"Unknown fight result {result}.");
                }
            }
            
            ui.EndFight(result);
            ChangeStage(stageIndex);
            ui.ChangeAdapter(this);

        }

        private void EndGame() //TODO:
        {
            
        }

        private Stage GetCurrentStage()
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