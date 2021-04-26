using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
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
        private int checkpointStageIndex;
        public Game game;
        private MainWindow ui;
        private const string FILENAME = @"game.json";
        public Stager(MainWindow window)
        {
            ui = window;
            stages = JsonInit(FILENAME);

            currentStageIndex = 0;
            DisplayStage(GetCurrentStage());
            game = new Game(new Magician("Me", Race.HUMAN, Sex.FEMALE, 17, 1000, 0, 1000));
        }

        public void GetInput(int index)
        {
            HandleAnswer(index);
            //ChangeStage(index);
        }

        public void HandleAnswer(int answerIndex)
        {
            Stage oldStage = GetCurrentStage();
            //if (currentStage != null && currentStage.Answers.Count > answerIndex && currentStage.Next.Count > answerIndex)
            if (oldStage != null && oldStage.Next.Count > answerIndex)
            {
                ChangeStage(oldStage.Next[answerIndex] - 1);
                
                if (oldStage.Actions.ContainsKey(answerIndex.ToString()))
                {
                    foreach (var action in oldStage.Actions[answerIndex.ToString()])
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
                ui.ChangeNumberOfButtons(stage.Next.Count);
                ui.ChangeButtonsText(stage.Answers);
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
                        ui.ChangeAdapter(fighter);
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
                    //object createdObject = GetObjectFromString(actionsWords[1], SubArray(actionsWords, 2, actionsWords.Length - 2));

                    //if (createdObject is Spell)
                    //{
                    //    game.hero.LearnSpell(createdObject as Spell);
                    //}
                    
                    break;
                }
                case "info": {
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
                case "compexp":
                {
                    string stageIndexString = game.hero.CompareTo(/* change */ game.hero) >= 0 ? actionsWords[1] : actionsWords[2];

                    if (Int32.TryParse(stageIndexString, out var stageIndex))
                    {
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
                                                $"StageIndex:{currentStageIndex}");
            }
        }

        private object GetObjectFromString(string className, string[] parameters)
        {
            Type type = Type.GetType(className);
            if (type == null)
            {
                throw new ArgumentException($"Type with specified name wasn't found - {className}. " +
                                                 $"StageIndex:{currentStageIndex}");
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
                                            $"StageIndex: {currentStageIndex}");
            }

            return createdObject;
        }

        public void StartFight()
        {
            ui.StartFight();
        }

        public void EndFight(FightResult result)
        {
            ui.EndFight(result);
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