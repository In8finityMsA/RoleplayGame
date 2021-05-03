using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using KashTaskWPF.Artifacts;
using KashTaskWPF.Adapters;
using KashTaskWPF.Spells;

namespace KashTaskWPF.Adapters
{
    public class Stager : IAdapter
    {
        private List<Stage> stages;
        private int currentStageIndex;
        private int previousStageIndex;

        private Magician heroSave;
        
        //Used when a fight has ended
        private int fightWonStage;
        private int fightDiedStage;
        private int fightRanStage;
        private int fightNegotiatedStage;

        private string lastUserTextInput;
        
        internal Magician hero;
        internal MainWindow ui;
        private const string FILENAME_PLOT = FOLDER_RESOURCES + "game.json";
        private const string FILENAME_HERO = FOLDER_RESOURCES + "hero.json";
        private const string FOLDER_RESOURCES = @"Resources/";
        private const string TEXTBOX_KEYWORD = "<TEXTBOX>"; //Magic word from json for displaying textbox instead of buttons

        //Fight result messages
        private const string WON_MESSAGE = "Об этой битве будут слагать легенды!";
        private const string RAN_MESSAGE = "Иногда лучше отступить, чтобы напасть вновь.";
        private const string DIED_MESSAGE = "Вы погибли... Не расстраивайтесь, попробуйте снова :)";
        private const string NEGOTIATED_MESSAGE = "Зачем махать кулаками, если можно поговорить.";

        //Abnoraml sutuations
        private const string FILE_PLOT_NOT_FOUND =
            "Файл сюжета не наден. Пожалуйста, убедитесь что в папке Resources находится game.json.";
        private const string FILE_RESOURCE_NOT_FOUND = "Файл ресурса не наден. " + 
        "Пожалуйста, убедитесь что в папке с программой существует папка Resources и в ней есть все необходимые файлы.";
        private const string INVALID_INPUT = "Некорректный ввод. Попробуйте снова.";

        private const string NEXT_MESSAGE = "Далее";
        private const string END_GAME = "Спасибо, что прошли нашу игру до конца. Хорошего дня :)";
        public Stager(MainWindow window)
        {
            ui = window;
            stages = JsonInit(FILENAME_PLOT);

            currentStageIndex = 0;
            DisplayStage(GetCurrentStage());
            var jsonDocument = JsonDocumentFromFile(FILENAME_HERO);
            hero = JsonParseMagician(jsonDocument.RootElement);
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
                ChangeStage(currentStage.Next[answerIndex]);
                
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
                if (stage.Image != null) ChangeImage(FOLDER_RESOURCES + stage.Image);
                
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

        private void ChangeImage(string filename)
        {
            if (!File.Exists(filename))
            {
                MessageBox.Show(FILE_RESOURCE_NOT_FOUND);
                throw new FileNotFoundException($"There is no file with specified name - {filename}.");
            }
            
            ui.ChangeImage(filename);
        }

        private void DoAction(string actionName)
        {
            actionName = actionName.Trim();
            var actionsWords = actionName.Split(' ');
            string command = actionsWords[0].ToLower();
            Console.WriteLine(command);
            switch (command)
            {
                case "fight": //fight <fightplan filename> <image> <won stage id> <died stage id> <ran stage id> <negotiated stage id>
                {
                    heroSave = new Magician(hero); //saves hero to restore if died
                    if (actionsWords.Length < 6) 
                        throw new ArgumentException($"Not enough parameters for {actionName} action. StageIndex: {previousStageIndex}");
                    
                    if (File.Exists(FOLDER_RESOURCES + actionsWords[1]))
                    {
                        var plan = JsonParseFight(FOLDER_RESOURCES + actionsWords[1]);
                        Fighter fighter = new Fighter(this, plan);
                        ui.ShowFightInfo();
                        ui.ChangeAdapter(fighter);
                        ChangeImage(FOLDER_RESOURCES + actionsWords[2]);
                    }
                    else throw new ArgumentException($"There is no fight plan with specified filename - {actionsWords[1]}" +
                                                     $"Stage: {previousStageIndex}");
                    
                    if (Int32.TryParse(actionsWords[3], out fightWonStage) &&
                        Int32.TryParse(actionsWords[4], out fightDiedStage) &&
                        Int32.TryParse(actionsWords[5], out fightRanStage) &&
                        Int32.TryParse(actionsWords[6], out fightNegotiatedStage))
                    {
                        if (fightWonStage >= stages.Count || fightRanStage >= stages.Count || fightNegotiatedStage >= stages.Count)
                        {
                            throw new ArgumentException($"There is no stage with one or both specified IDs - " +
                                                        $"{actionsWords[3]}, {actionsWords[4]}, {actionsWords[5]}. " +
                                                        $"StageIndex:{previousStageIndex}");
                        }
                    }
                    else
                        throw new ArgumentException($"One or both stage IDs are incorrect (not integers) - " +
                                                    $"{actionsWords[3]}, {actionsWords[4]}, {actionsWords[5]}. " +
                                                    $"StageIndex:{previousStageIndex}");
                    
                    break;
                }
                case "set": //set <property name> <property type> <value>
                {
                    SetPropertyAction(actionsWords);
                    break;
                }
                case "get": //get <artifact class name>
                {
                    if (actionsWords.Length < 2) 
                        throw new ArgumentException($"Not enough parameters for {actionName} action. StageIndex: {previousStageIndex}");

                    Artifact artifact = ArtifactFromString(actionsWords[1],
                        GameUtils.SubArray(actionsWords, 2, actionsWords.Length - 2));
                    if (artifact != null)
                        hero.PickUpArtifact(artifact);
                    else
                        throw new ArgumentException($"Constructed type is not an artifact. StageIndex: {previousStageIndex}");
                    
                    break;
                }
                case "learn":
                {
                    if (actionsWords.Length < 2) 
                        throw new ArgumentException($"Not enough parameters for {actionName} action. StageIndex: {previousStageIndex}");

                    Spell spell = SpellFromString(actionsWords[1],
                        GameUtils.SubArray(actionsWords, 2, actionsWords.Length - 2));
                    if (spell != null)
                        hero.LearnSpell(spell);
                    else
                        throw new ArgumentException($"Constructed type is not a spell. StageIndex: {previousStageIndex}");
                    
                    break;
                }
                case "info":
                case "repeat": //метод о выписывании инфы по персонажу выписывает инфу в окно
                { 
                    ui.ChangeText(ui.MainText.Text + "\n" + hero.ToString());
                    break; 
                }
                case "damage": // damage <value>  наносит урон игроку
                {
                    if (actionsWords.Length < 2) 
                        throw new ArgumentException($"Not enough parameters for {actionName} action. StageIndex: {previousStageIndex}");
                    
                    if (Int32.TryParse(actionsWords[1], out var amountDamage))
                    {
                        hero.Health -= amountDamage;
                    }
                    
                    break; 
                }
                case "getexp": // getexp <value>  дать игроку опыт
                {
                    if (actionsWords.Length < 2) 
                        throw new ArgumentException($"Not enough parameters for {actionName} action. StageIndex: {previousStageIndex}");
                    
                    if (Int32.TryParse(actionsWords[1], out var amountExp))
                    {
                        hero.Experience += amountExp;
                    }
                    
                    break; 
                }
                case "compexp": // compexp <character filename> <win stage id> <lose stage id>  сравнивает экспу игрока и гнома 
                {
                    //Make compare with number supplied by action?
                    if (actionsWords.Length < 4) 
                        throw new ArgumentException($"Not enough parameters for {actionName} action. StageIndex: {previousStageIndex}");

                    var jsonDocument = JsonDocumentFromFile(FOLDER_RESOURCES + actionsWords[1]);
                    Character characterToCompare = JsonParseCharacter(jsonDocument.RootElement);
                    string stageIndexString = hero.CompareTo(characterToCompare) >= 0 ? actionsWords[2] : actionsWords[3];

                    if (Int32.TryParse(stageIndexString, out var stageIndex))
                    {
                        ChangeStage(stageIndex);
                    }
                    else
                        throw new ArgumentException($"Specified stage id is not a number - {actionsWords[2]}, {actionsWords[3]}");
                    
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
                    hero = heroSave;
                    stageIndex = fightDiedStage;
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
            
            hero.StatesDynamic.Clear();
            hero.ActionsOnStep = null;
            ui.HideFightInfo();
            ChangeStage(stageIndex);
            ui.ChangeAdapter(this);

        }

        private void EndGame()
        {
            MessageBox.Show(END_GAME);
            ui.Close();
        }

        private Stage GetCurrentStage()
        {
            if (currentStageIndex < stages.Count)
            {
                return stages[currentStageIndex];
            }

            return null;
        }

        private void SetPropertyAction(string[] actionsWords)
        {
            if (actionsWords.Length < 4) 
                throw new ArgumentException($"Not enough parameters for set action. StageIndex:{previousStageIndex}");
                    
            var property = hero.GetType().GetProperty(actionsWords[1]);
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
                        property.SetValue(hero, value);
                        break;
                    }
                    case "string":
                    {
                        property.SetValue(hero, stringValue);
                        break;
                    }
                    case "int":
                    {
                        var value = Int32.Parse(stringValue);
                        property.SetValue(hero, value);
                        break;
                    }
                    case "Race":
                    {
                        var value = Enum.Parse<Race>(stringValue);
                        property.SetValue(hero, value);
                        break;
                    }
                    case "Sex":
                    {
                        var value = Enum.Parse<Sex>(stringValue);
                        property.SetValue(hero, value);
                        break;
                    }
                    default:
                        throw new ArgumentException(
                            $"Specified property type is not supported - {actionsWords[2]}" +
                            $"StageIndex:{previousStageIndex}");
                }
            }
            catch (ArgumentNullException)
            {
                throw new ArgumentException($"Specified value is null" +
                                            $"StageIndex:{previousStageIndex}");
            }
            catch (Exception ex) when (ex is TargetInvocationException || ex is FormatException)
            {
                if (actionsWords[3].Equals(TEXTBOX_KEYWORD))
                {
                    ChangeStage(previousStageIndex);
                    MessageBox.Show(INVALID_INPUT);
                }
                else
                {
                    throw new ArgumentException($"Specified value is incorrect - {actionsWords[3]}" +
                                                $"StageIndex:{previousStageIndex}");
                }
            }
        }

        private Artifact ArtifactFromString(string artifactName, string[] parameters)
        {
            object createdObject = GameUtils.GetObjectFromString("KashTaskWPF.Artifacts." + artifactName, parameters);

            if (createdObject is Artifact artifact)
                return artifact;

            return null;
        }
        
        private Spell SpellFromString(string spellName, string[] parameters)
        {
            object createdObject = GameUtils.GetObjectFromString("KashTaskWPF.Spells." + spellName, parameters);

            if (createdObject is Spell spell)
                return spell;

            return null;
        }

        private List<Stage> JsonInit(string filename)
        {
            List<Stage> deserialize = null;
            if (!File.Exists(filename))
            {
                MessageBox.Show(FILE_PLOT_NOT_FOUND);
                throw new FileNotFoundException($"There is no file with specified name - {filename}.");
            }

            var reader = new StreamReader(filename);
            var jsonString = reader.ReadToEnd();
            reader.Close();
            deserialize = JsonSerializer.Deserialize<List<Stage>>(jsonString);
            
            return deserialize;
        }

        private JsonDocument JsonDocumentFromFile(string filename)
        {
            if (!File.Exists(filename))
            {
                MessageBox.Show(FILE_RESOURCE_NOT_FOUND);
                throw new FileNotFoundException($"There is no file with specified name - {filename}.");
            }
            
            var reader = new StreamReader(filename);
            var jsonString = reader.ReadToEnd();
            reader.Close();
            return JsonDocument.Parse(jsonString);
        }

        private FightPlan JsonParseFight(string filename)
        {
            FightPlan fightPlan = new FightPlan();
            
            var jsonDocument = JsonDocumentFromFile(filename);
            var root = jsonDocument.RootElement;
            if (root.TryGetProperty("EnemyCharacters", out var enemies))
            {
                foreach (JsonElement element in enemies.EnumerateArray())
                {
                    var character = JsonParseCharacter(element);
                    fightPlan.EnemyList.Add(character);
                }
            }
            
            if (root.TryGetProperty("EnemyMagicians", out var enemiesM))
            {
                foreach (JsonElement element in enemiesM.EnumerateArray())
                {
                    var magician = JsonParseMagician(element);
                    fightPlan.EnemyList.Add(magician);
                }
            }

            if (root.TryGetProperty("EnemyWords", out var enemyWordsElement))
            {
                foreach (var word in enemyWordsElement.EnumerateArray())
                {
                    fightPlan.enemiesWord.Add(word.GetString());
                }
            }
            
            if (root.TryGetProperty("HeroWords", out var heroWordsElement))
            {
                foreach (var wordArray in heroWordsElement.EnumerateArray())
                {
                    List<string> list = new List<string>();
                    foreach (var word in wordArray.EnumerateArray())
                    {
                        list.Add(word.GetString());
                    }

                    fightPlan.yourWord.Add(list);
                }
            }

            if (root.TryGetProperty("EXP", out var exp))
            {
                fightPlan.EXP = exp.GetInt32();
            }
            
            return fightPlan;
        }

        private Character JsonParseCharacter(JsonElement characterElement)
        {
            //Mandatory part
            string name = characterElement.GetProperty("Name").GetString();
            Race race = Enum.Parse<Race>(characterElement.GetProperty("Race").GetString());
            Sex sex = Enum.Parse<Sex>(characterElement.GetProperty("Sex").GetString());
                    
            //Optional part
            int age = Character.DEFAULT_AGE;
            double maxHealth = Character.DEFAULT_MAXHEALTH;
            int experience = Character.DEFAULT_EXPERIENCE;
            double hitPower = Character.DEFAULT_HITPOWER;
            if (characterElement.TryGetProperty("Age", out var propAge))
            {
                age = propAge.GetInt32();
            }
            if (characterElement.TryGetProperty("MaxHealth", out var propMaxHealth))
            {
                maxHealth = propMaxHealth.GetDouble();
            }
            if (characterElement.TryGetProperty("Experience", out var propExp))
            {
                experience = propExp.GetInt32();
            }
            if (characterElement.TryGetProperty("HitPower", out var propHitPower))
            {
                hitPower = propHitPower.GetDouble();
            }

            Character character = new Character(name, race, sex, age, maxHealth, experience, hitPower);
                    
            //Inventory optional part
            if (characterElement.TryGetProperty("Inventory", out var propInventory))
            {
                foreach (var itemElement in propInventory.EnumerateArray())
                {
                    var itemStringArray = itemElement.GetString().Split(' ');
                    Artifact artifact = ArtifactFromString(itemStringArray[0], GameUtils.SubArray(itemStringArray, 1, itemStringArray.Length - 1) );
                    if (artifact != null)
                        character.PickUpArtifact(artifact);
                    else
                        throw new ArgumentException($"Constructed type is not an artifact.");
                }
            }

            return character;
        }

        private Magician JsonParseMagician(JsonElement magicianElement)
        {
            var character = JsonParseCharacter(magicianElement);
            //Mandatory part
            double maxMana = magicianElement.GetProperty("MaxMana").GetDouble();

            Magician magician = new Magician(character, maxMana);
                    
            //Spells optional part
            if (magicianElement.TryGetProperty("Spells", out var propSpells))
            {
                foreach (var spellElement in propSpells.EnumerateArray())
                {
                    var spellStringArray = spellElement.GetString().Split(' ');
                    Spell spell = SpellFromString(spellStringArray[0], GameUtils.SubArray(spellStringArray, 1, spellStringArray.Length - 1) );
                    if (spell != null)
                        magician.LearnSpell(spell);
                    else
                        throw new ArgumentException($"Constructed type is not a spell.");
                }
            }

            return magician;
        }
        
        public class Stage
        {
            public Stage() { }
        
            public int ID { get; set; }
            public string Text { get; set; }
            public List<string> Answers { get; set; }
            public Dictionary<string, List<string>> Actions { get; set; }
            public string Image { get; set; }
            public List<int> Next { get; set; }
        }
        
    }
}