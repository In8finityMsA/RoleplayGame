using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using game;
using KASHGAMEWPF;
using KashTaskWPF;
using KashTaskWPF.Adapters;

namespace KashTask
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
            game = new Game(new Magician("Me", Race.HUMAN, Sex.FEMALE, 17, 100, 0, 50));
        }

        public void GetInput(int index)
        {
            ChangeStage(index);
        }


        public void ChangeStage(int answerIndex)
        {
            Stage currentStage = GetCurrentStage();
            //if (currentStage != null && currentStage.Answers.Count > answerIndex && currentStage.Next.Count > answerIndex)
            if (currentStage != null && currentStage.Next.Count > answerIndex)
            {
                if (currentStage.Actions.ContainsKey(answerIndex.ToString()))
                {
                    foreach (var action in currentStage.Actions[answerIndex.ToString()])
                    {
                        //DoAction(action);
                        Console.WriteLine("Action");
                    }
                }
                currentStageIndex = currentStage.Next[answerIndex] - 1;
                Stage newStage = GetCurrentStage();
                if (newStage != null)
                {
                    ui.ChangeText(newStage.Text );
                    ui.ChangeNumberOfButtons(newStage.Next.Count);
                    ui.ChangeButtonsText(newStage.Answers);
                }
            }
        }

        private void DoAction(string actionName)
        {
            actionName = actionName.ToLower().Trim();
            var actionsWords = actionName.Split(' ');
            string command = actionsWords[0];
            switch (command)
            {
                case "fight":
                {                  
                        StartFight();
                        List<FightPlan> fightplans = game.fightPlans;
                        Fighter fighter = new Fighter(this, fightplans[0]);
                        fightplans.RemoveAt(0);
                        ui.ChangeAdapter(fighter);
                        break;
                }
                case "get": break;
                case "learn": break;
                case "repeat": break; //метод о выписывании инфы по персонажу выписывает инфу в окно
                case "damage": break; // наносит урон игроку
                case "getexp": break; //дать игроку опыт
                case "camp": break; // то самое окно, где можно учить спеллы и открыть инвентарь
                case "compexp": break; //сравнивает экспу игрока и гнома
                case "end": break; //конец игры
                default:
                    throw new ArgumentException($"There is no action with specified name - {actionName}. " +
                                                $"StageIndex:{currentStageIndex}");
            }
        }

        public void StartFight()
        {
            ui.StartFight();
        }

        public void EndFight(FightResult result)
        {
            ui.EndFight(result);
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
            Console.WriteLine(deserialize?.Count);
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
            }
            
            return deserialize;
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