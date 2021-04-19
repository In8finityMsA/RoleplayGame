using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using game;

namespace KashTask
{
    public class Stager
    {
        private List<Stage> stages;
        private int currentStageIndex;
        private int checkpointStageIndex;
        private Magician player;
        private const string FILENAME = @"game.json";
        public Stager()
        {
            JsonInit(FILENAME);
        }

        public void ChangeStage(int answerIndex)
        {
            Stage currentStage = GetCurrentStage();
            if (currentStage.Answers.Count > answerIndex && currentStage.Next.Count > answerIndex)
            {
                if (currentStage.Actions.ContainsKey(answerIndex.ToString()))
                {
                    foreach (var action in currentStage.Actions[answerIndex.ToString()])
                    {
                        DoAction(action);
                    }
                }
                currentStageIndex = currentStage.Next[answerIndex];
            }
        }

        private void DoAction(string actionName)
        {
            actionName = actionName.ToLower();
            switch (actionName)
            {
                case "fight": break;
                case "get": break;
                case "learn": break;
                default:
                    throw new ArgumentException($"There is no action with specified name - {actionName}. " +
                                                $"StageIndex:{currentStageIndex}");
            }
        }

        public Stage GetCurrentStage()
        {
            return stages[currentStageIndex];
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