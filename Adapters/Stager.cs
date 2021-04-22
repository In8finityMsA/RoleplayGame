using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Artifacts;
using game;
using KashTaskWPF;
using KashTaskWPF.Adapters;
using Microsoft.Windows.Themes;

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
            game = new Game(new Magician("Me", Race.HUMAN, Sex.FEMALE, 17, 100, 0, 10));
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
                    //var ctorInfo = Type.GetType(actionsWords[1]).GetConstructor();
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
                case "repeat": break; //метод о выписывании инфы по персонажу выписывает инфу в окно
                case "damage":
                {
                    int amountDamage;
                    if (Int32.TryParse(actionsWords[1], out amountDamage))
                    {
                        game.hero.Health -= amountDamage;
                    }
                    
                    break; // наносит урон игроку
                }
                case "getexp": 
                {
                    int amountExp;
                    if (Int32.TryParse(actionsWords[1], out amountExp))
                    {
                        game.hero.Experience += amountExp;
                    }
                    
                    break; //дать игроку опыт
                }
                case "camp": break; // то самое окно, где можно учить спеллы и открыть инвентарь
                case "compexp":
                {
                    string stageIndexString;
                    stageIndexString = game.hero.CompareTo(/* change */ game.hero) >= 0 ? actionsWords[1] : actionsWords[2]; //TODO: change game.hero with a character to compare to

                    int stageIndex;
                    if (Int32.TryParse(stageIndexString, out stageIndex))
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
            Type type = Type.GetType("Artifacts." + className);
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
                throw new ArgumentException($"Constructor with specified parameters for type {className} wasn't found. " +
                                            $"Parameters: {parameters}. " +
                                            $"StageIndex:{currentStageIndex}");
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
            /*Console.WriteLine(deserialize?.Count);
            foreach (var entry in deserialize)
            {
                Console.WriteLine(entry.ID);
                Console.WriteLine(entry.Text);
                entry.Answers.ForEach(i => Console.Write("{0}, ", i));
                Console.WriteLine();
                entry.Next.ForEach(i => Console.Write("{0}, ", i));
                Console.WriteLine();
                foreach (var index in entry.Actions)
                {   
                    index.Value.ForEach(i => Console.Write("{0}, ", i));
                    Console.WriteLine();
                }
                Console.WriteLine();
            }*/
            
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