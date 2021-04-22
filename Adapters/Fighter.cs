using game;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using KashTaskWPF.Adapters;
using KashTaskWPF;
using Artifacts;

namespace KashTaskWPF.Adapters
{
    public enum FightResult
    {
        WON, DIED, RAN
    }
    public enum FightStatus
    {
        ChooseAction, ChooseTarget, ChoosePower, ChooseSpell, ChooseArtifact, ChooseWords
    }
    public enum FightAction
    {
        HIT = 1, SPELL = 2, ARTIFACT = 3, TALK = 4, RUN = 5
    }
    public class Fighter: IAdapter
    {
        //private readonly string fileName;
        private FightPlan plan;
        private List<string> prevList;
        private int prevVariantsAmount;
        private List<string> StandartList = new List<string>() { "Удар", "Заклинание", "Артефакт", "Поговорить", "Бежать" };
        private List<Character> enemies = new List<Character>();
        List<KeyValuePair<Type, Spell>> spells;
        List<Artifact> artifacts;
        List<string> words;
        List<string> answers;

        private string CHOOSETARGET = "Вы можете выбрать цель, на которую хотите направить свое действие!";
        private string CHOOSEACTION = "Выберете действие, которое хотите направить на врагов!";
        private string CHOOSEPOWER = "Выберете силу действия";
        private string CHOOSESPELL = "Выберете заклинание!";
        private string CHOOSEARTIFACT = "Выберете артефакт!";
        private string CHOOSEANOTHERACTION = "Выберете другое действие!";
        private string NOMANA = "У вас нет маны!";
        private string NOARTIFACTS = "У вас нет артефактов!";
        private string NOSPELLS = " У вас нет выученных заклинаний!";
        private string NOTENOUGHMANA = "Вам не хватило маны на заклинание! Штош, придется пропустить ход(";
        private string CONVERSATION = "";

        private string ABOUTENEMYPUNCHES = "";
        private Stager parent;
        private MainWindow ui = KashTaskWPF.MainWindow.mainwindow;

        private FightAction whatNow;
        private FightStatus prevStatus = FightStatus.ChooseAction;
        private FightStatus chooseParams = FightStatus.ChooseAction;

        private Character target;
        private Spell spell;
        private double power;
        private Artifact artifact;



        public Fighter(/*string fileName,*/ Stager parent, FightPlan plan)
        {
            //this.fileName = fileName;
            this.parent = parent;
            this.plan = plan;
            
            spells = ((Magician)parent.game.hero).Spells.ToList<KeyValuePair<Type, Spell>>();
            artifacts = parent.game.hero.Inventory;

            words = plan.yourWord;
            answers = plan.enemiesWord;

            ui.InfoAboutCurrentConditions("Вы можете выбрать действие, чтобы атаковать врага!");

            //JsonInit();
            JoinLists();
            ui.GetInfo(StandartList, 5);

            ui.GetInfoEnemies(enemies);
            ui.GetInfoCharacter(parent.game.hero);
        }

        private void JoinLists()
        {
            List<Character> magaschar = new List<Character>(plan.enemyM.Count);
            for (int i = 0; i < plan.enemyM.Count; i++)
            {
                magaschar.Add((Character)plan.enemyM[i]);
            }
            enemies.AddRange(magaschar);
            enemies.AddRange(plan.enemyNonM);
        }

        //public void JsonInit()
        //{
        //    var reader = new StreamReader(fileName);
        //    var jsonString = reader.ReadToEnd();
        //    reader.Close();
        //    plan = JsonSerializer.Deserialize<FightPlan>(jsonString);
        //}


        public List<string> EnemyNamesToList()
        {
            List<string> enemiesNames = new List<string>();

            for (int i = 0; i < enemies.Count; i++)
            {
                enemiesNames.Add(enemies[i].Name);
            }
            return enemiesNames;
        }

        public List<string> PowerToList()
        {
            return new List<string>() { "10", "20", "30" , "40", "50"};
        }

        public List<string> SpellNamesToList()
        {
            List<string> spellsNames = new List<string>();
            for (int i = 0; i < spells.Count; i++)
            {
                spellsNames.Add(spells[i].Value.NAME);
            }
            return spellsNames;
        }

        public List<string> ArtifactNamesToList()
        {
            artifacts = parent.game.hero.Inventory;
            List<string> artifactNames = new List<string>();
            for (int i = 0; i < artifacts.Count; i++)
            {
                artifactNames.Add(artifacts[i].NAME);
            }
            return artifactNames;
        }

        private void WritePrevInfo(List<string> list, int amount)
        {
            prevList = list;
            prevVariantsAmount = amount;

        }

        public void GetInput(int index)
        {
            //index += 1;
            if (chooseParams == FightStatus.ChooseAction)////// if gamer has to choose action
            {
                index += 1;
                whatNow = (FightAction)(index);               
                switch (index)
                {
                    case 1: //HIT
                        {
                            if ( enemies.Count > 1)
                            {
                                prevStatus = chooseParams;
                                chooseParams = FightStatus.ChooseTarget;
                                ui.InfoAboutCurrentConditions(CHOOSETARGET);

                                ui.GetInfo(EnemyNamesToList(), enemies.Count);//providing gamer with options of enemies                   
                            }                           
                            else if (enemies.Count == 1)
                            {
                                parent.game.hero.Hit(enemies[0]);                                

                                if (enemies[0].StateHealth == StateHealth.DEAD)
                                {   
                                    enemies.RemoveAt(0);
                                    ui.GetInfoEnemies(enemies);
                                    parent.EndFight(FightResult.WON);
                                    return;
                                }
                                else
                                {
                                    YourEnemyReaction();

                                    if (parent.game.hero.StateHealth == StateHealth.DEAD)
                                    {                        
                                        parent.EndFight(FightResult.DIED);
                                        return;
                                    }
                                }
                                //prevStatus = chooseParams;
                                ui.InfoAboutCurrentConditions(ABOUTENEMYPUNCHES + CHOOSEACTION);
                                chooseParams = FightStatus.ChooseAction;
                                ui.GetInfo(StandartList, StandartList.Count);                                                                
                            }                         
                        }                       
                        break;
                    case 2: //SPELL
                        {                         
                            if (spells.Count >= 1)
                            {
                                if (parent.game.hero.Mana == 0)
                                {
                                    ui.InfoAboutCurrentConditions(NOMANA + '\n' + CHOOSEANOTHERACTION);
                                }
                                else
                                {
                                    //prevStatus = chooseParams;
                                    chooseParams = FightStatus.ChooseSpell;
                                    ui.InfoAboutCurrentConditions(CHOOSESPELL);
                                    ui.GetInfo(SpellNamesToList(), spells.Count);
                                }                              
                            }
                            else
                            {
                                ui.InfoAboutCurrentConditions(NOSPELLS + '\n' + CHOOSEANOTHERACTION);
                            }                            
                        }
                        break;
                    case 3:  //ARTIFACT
                        {                            
                            if (artifacts.Count >= 1)
                            {
                                //prevStatus = chooseParams;
                                ui.InfoAboutCurrentConditions(CHOOSEARTIFACT);
                                chooseParams = FightStatus.ChooseArtifact;
                                ui.GetInfo(ArtifactNamesToList(), artifacts.Count);
                            }
                            else
                            {
                                ui.InfoAboutCurrentConditions(NOARTIFACTS + '\n' + CHOOSEANOTHERACTION);
                            }
                        }
                        break;
                    case 4:  //TALK
                        {
                            //if (words.Count == 0)
                            //{
                            //    ui.InfoAboutCurrentConditions(CHOOSEACTION);
                            //    chooseParams = FightStatus.ChooseAction;
                            //    CheckStandartList();
                            //    ui.GetInfo(StandartList, StandartList.Count);
                            //}
                            //else
                            //{
                            //    prevStatus = chooseParams;
                            //    //ui.InfoAboutCurrentConditions(CHOOSEWORDS);
                            //    chooseParams = FightStatus.ChooseWords;
                            //    ui.GetInfo(new List<string>() { words[0] }, 1);
                            //    words.RemoveAt(0);
                            //}
                        }
                        break;
                    case 5: //RUN
                        {
                            //parent.EndFight(FightResult.RAN);
                        }
                        break;
                    default:
                        
                        break;
                }                
            }
            else if (chooseParams == FightStatus.ChooseSpell)
            {
                spell = spells[index].Value;
                if (spell is IMagicPowered)
                {
                    //prevStatus = chooseParams;
                    ui.InfoAboutCurrentConditions(CHOOSEPOWER);
                    chooseParams = FightStatus.ChoosePower;
                    ui.GetInfo(PowerToList(), 5);
                }
                else
                {
                    if (enemies.Count != 1)
                    {
                        //prevStatus = chooseParams;
                        ui.InfoAboutCurrentConditions(CHOOSETARGET);
                        chooseParams = FightStatus.ChooseTarget;
                        ui.GetInfo(EnemyNamesToList(), enemies.Count);
                    }
                    else if (enemies.Count == 1)
                    {
                        target = enemies[0];
                        try
                        {
                            ((Magician)parent.game.hero).UseSpell(spell, target);
                        }
                        catch (Exception)
                        {
                            ui.InfoAboutCurrentConditions(NOTENOUGHMANA + '\n' + CHOOSEACTION);
                            chooseParams = FightStatus.ChooseAction;
                            ui.GetInfo(StandartList, StandartList.Count);
                        }
                        
                        ui.GetInfoEnemies(enemies);
                        ui.GetInfoCharacter(parent.game.hero);
                        if (enemies[0].StateHealth == StateHealth.DEAD)
                        {
                            enemies.RemoveAt(0);
                            ui.GetInfoEnemies(enemies);
                            parent.EndFight(FightResult.WON);
                            return;
                        }
                        else
                        {
                            YourEnemyReaction();
                            if (parent.game.hero.StateHealth == StateHealth.DEAD)
                            {                         
                                parent.EndFight(FightResult.DIED);
                                return;
                            }
                        }
                        //prevStatus = chooseParams;
                        ui.InfoAboutCurrentConditions(ABOUTENEMYPUNCHES + CHOOSEACTION);
                        chooseParams = FightStatus.ChooseAction;
                        ui.GetInfo(StandartList, StandartList.Count);
                        
                    }
                }
            }
            else if (chooseParams == FightStatus.ChooseArtifact)
            {
                artifact = artifacts[index];

                if (artifact is IMagicPowered)
                {
                    //prevStatus = chooseParams;
                    ui.InfoAboutCurrentConditions(CHOOSEPOWER);
                    chooseParams = FightStatus.ChoosePower;
                    ui.GetInfo(PowerToList(), 5);
                }
                else
                {
                    if (enemies.Count >= 1)
                    {
                        //prevStatus = chooseParams;
                        ui.InfoAboutCurrentConditions(CHOOSETARGET);
                        chooseParams = FightStatus.ChooseTarget;
                        ui.GetInfo(EnemyNamesToList(), enemies.Count);
                    }
                    else if (enemies.Count == 1)
                    {
                        target = enemies[0];

                        try
                        {
                            ((Magician)parent.game.hero).UseArtifact(artifact, target);
                        }
                        catch (Exception)
                        {
                            ui.InfoAboutCurrentConditions(NOTENOUGHMANA + '\n' + CHOOSEACTION);
                            chooseParams = FightStatus.ChooseAction;
                            ui.GetInfo(StandartList, StandartList.Count);
                        }
                        
                        ui.GetInfoEnemies(enemies);
                        if (enemies[0].StateHealth == StateHealth.DEAD)
                        {
                            enemies.RemoveAt(0);
                            ui.GetInfoEnemies(enemies);
                            //tell about murder?
                            parent.EndFight(FightResult.WON);
                            return;
                        }
                        else
                        {
                            YourEnemyReaction();

                            if (parent.game.hero.StateHealth == StateHealth.DEAD)
                            {                          
                                parent.EndFight(FightResult.DIED);
                                return;
                            }
                        }
                        prevStatus = chooseParams;
                        ui.InfoAboutCurrentConditions(ABOUTENEMYPUNCHES + CHOOSEACTION);
                        chooseParams = FightStatus.ChooseAction;
                        ui.GetInfo(StandartList, StandartList.Count);                      
                    }
                }
            }
            else if (chooseParams == FightStatus.ChooseTarget)
            {
                target = enemies[index];

                if (whatNow == FightAction.HIT)
                {
                    parent.game.hero.Hit(target);
                    ui.GetInfoEnemies(enemies);
                    if (target.StateHealth == StateHealth.DEAD)
                    {
                        enemies.Remove(target);
                        ui.GetInfoEnemies(enemies);
                        //tell UI about murder?
                        if (enemies.Count == 0)
                        {
                            parent.EndFight(FightResult.WON);
                            return;
                        }
                    }
                    else
                    {
                        YourEnemyReaction();
                        if (parent.game.hero.StateHealth == StateHealth.DEAD)
                        {                           
                            parent.EndFight(FightResult.DIED);
                            return;
                        }
                    }
                    prevStatus = chooseParams;
                    ui.InfoAboutCurrentConditions(ABOUTENEMYPUNCHES + CHOOSEACTION);
                    chooseParams = FightStatus.ChooseAction;
                    ui.GetInfo(StandartList, StandartList.Count);
                }
                else if (whatNow == FightAction.SPELL)
                {
                    if (spell is IMagicPowered)
                    {
                        try
                        {
                            ((Magician)parent.game.hero).UseSpell(spell, target, power);
                        }
                        catch (Exception)
                        {
                            ui.InfoAboutCurrentConditions(NOTENOUGHMANA + '\n' + CHOOSEACTION);
                            chooseParams = FightStatus.ChooseAction;
                            ui.GetInfo(StandartList, StandartList.Count);
                        }

                        ui.GetInfoCharacter(parent.game.hero);
                        ui.GetInfoEnemies(enemies);
                        if (target.StateHealth == StateHealth.DEAD)
                        {
                            enemies.Remove(target);
                            ui.GetInfoEnemies(enemies);
                            //tell UI about murder?
                            if (enemies.Count == 0)
                            {
                                parent.EndFight(FightResult.WON);
                                return;
                            }
                        }
                        else
                        {
                            YourEnemyReaction();
                            if (parent.game.hero.StateHealth == StateHealth.DEAD)
                            {                           
                                parent.EndFight(FightResult.DIED);
                                return;
                            }
                        }
                        prevStatus = chooseParams;
                        ui.InfoAboutCurrentConditions(ABOUTENEMYPUNCHES + CHOOSEACTION);
                        chooseParams = FightStatus.ChooseAction;
                        ui.GetInfo(StandartList, StandartList.Count);
                    }
                    else
                    {
                        try
                        {
                            ((Magician)parent.game.hero).UseSpell(spell, target);
                        }
                        catch
                        {
                            ui.InfoAboutCurrentConditions(NOTENOUGHMANA + '\n' + CHOOSEACTION);
                            chooseParams = FightStatus.ChooseAction;
                            ui.GetInfo(StandartList, StandartList.Count);
                        }

                        ui.GetInfoCharacter(parent.game.hero);
                        ui.GetInfoEnemies(enemies);
                        if (target.StateHealth == StateHealth.DEAD)
                        {
                            enemies.Remove(target);
                            ui.GetInfoEnemies(enemies);
                            //tell UI about murder?
                            if (enemies.Count == 0)
                            {
                                parent.EndFight(FightResult.WON);
                                return;
                            }
                        }
                        else
                        {
                            YourEnemyReaction();
                            if (parent.game.hero.StateHealth == StateHealth.DEAD)
                            {                       
                                parent.EndFight(FightResult.DIED);
                                return;
                            }
                        }
                        prevStatus = chooseParams;
                        ui.InfoAboutCurrentConditions(ABOUTENEMYPUNCHES + CHOOSEACTION);
                        chooseParams = FightStatus.ChooseAction;
                        ui.GetInfo(StandartList, StandartList.Count);
                    }

                }
                else if (whatNow == FightAction.ARTIFACT)
                {
                    if (artifact is IMagicPowered)
                    {
                        parent.game.hero.UseArtifact((PoweredRenewableArtifact)artifact, target, power);
                        ui.GetInfoEnemies(enemies);
                        if (target.StateHealth == StateHealth.DEAD)
                        {
                            enemies.Remove(target);
                            ui.GetInfoEnemies(enemies);
                            //tell UI about murder?
                            if (enemies.Count == 0)
                            {
                                parent.EndFight(FightResult.WON);
                                return;
                            }
                        }
                        else
                        {
                            YourEnemyReaction();
                            if (parent.game.hero.StateHealth == StateHealth.DEAD)
                            {                          
                                parent.EndFight(FightResult.DIED);
                                return;
                            }
                        }
                        //prevStatus = chooseParams;
                        ui.InfoAboutCurrentConditions(ABOUTENEMYPUNCHES + CHOOSEACTION);
                        chooseParams = FightStatus.ChooseAction;
                        ui.GetInfo(StandartList, StandartList.Count);
                    }
                    else
                    {
                        parent.game.hero.UseArtifact(artifact, target);
                        ui.GetInfoEnemies(enemies);
                        if (target.StateHealth == StateHealth.DEAD)
                        {
                            enemies.Remove(target);
                            ui.GetInfoEnemies(enemies);
                            //tell UI about murder?
                            if (enemies.Count == 0)
                            {
                                parent.EndFight(FightResult.WON);
                                return;
                            }
                        }
                        else
                        {
                            YourEnemyReaction();
                            if (parent.game.hero.StateHealth == StateHealth.DEAD)
                            {                           
                                parent.EndFight(FightResult.DIED);
                                return;
                            }
                        }
                        //prevStatus = chooseParams;
                        ui.InfoAboutCurrentConditions(ABOUTENEMYPUNCHES + CHOOSEACTION);
                        chooseParams = FightStatus.ChooseAction;
                        ui.GetInfo(StandartList, StandartList.Count);
                    }
                }
                else if (whatNow == FightAction.TALK)
                {

                }
                
            }
            else if (chooseParams == FightStatus.ChooseWords)
            {

            }
            else if (chooseParams == FightStatus.ChoosePower)/////////////////////////////////////////////////////////////////////////////
            {
                index += 1;
                power = index * 10;
                if (enemies.Count > 1)
                {
                    //prevStatus = chooseParams;
                    ui.InfoAboutCurrentConditions(CHOOSETARGET);
                    chooseParams = FightStatus.ChooseTarget;
                    ui.GetInfo(EnemyNamesToList(), enemies.Count);
                    //ui.GetInfo(PowerToList(), 5);
                }
                else if (enemies.Count == 1)
                {
                    if (whatNow == FightAction.SPELL)
                    {
                        try
                        {
                            ((Magician)parent.game.hero).UseSpell(spell, enemies[0], power);
                        }
                        catch (Exception)
                        {
                            ui.InfoAboutCurrentConditions(NOTENOUGHMANA + '\n' + CHOOSEACTION);
                            chooseParams = FightStatus.ChooseAction;
                            ui.GetInfo(StandartList, StandartList.Count);
                        }
                        ui.GetInfoCharacter(parent.game.hero);
                        ui.GetInfoEnemies(enemies);
                        if (target.StateHealth == StateHealth.DEAD)
                        {
                            enemies.RemoveAt(0);
                            //tell UI about murder?
                            ui.GetInfoEnemies(enemies);
                            parent.EndFight(FightResult.WON);
                            return;
                        }
                        else
                        {
                            YourEnemyReaction();
                            if (parent.game.hero.StateHealth == StateHealth.DEAD)
                            {                          
                                parent.EndFight(FightResult.DIED);
                                return;
                            }
                        }
                        //prevStatus = chooseParams;
                        ui.InfoAboutCurrentConditions(ABOUTENEMYPUNCHES + CHOOSEACTION);
                        chooseParams = FightStatus.ChooseAction;
                        ui.GetInfo(StandartList, StandartList.Count);
                    }
                    else if (whatNow == FightAction.ARTIFACT)
                    {
                        parent.game.hero.UseArtifact((PoweredRenewableArtifact)artifact, enemies[0], power);
                        ui.GetInfoEnemies(enemies);
                        //enemy actions? Are you alive?
                        if (target.StateHealth == StateHealth.DEAD)
                        {
                            enemies.RemoveAt(0);
                            ui.GetInfoEnemies(enemies);
                            //tell UI about murder?                            
                            parent.EndFight(FightResult.WON);
                            return;                            
                        }
                        else
                        {
                            YourEnemyReaction();
                            if (parent.game.hero.StateHealth == StateHealth.DEAD)
                            {                         
                                parent.EndFight(FightResult.DIED);
                                return;
                            }
                        }
                        //prevStatus = chooseParams;
                        ui.InfoAboutCurrentConditions(ABOUTENEMYPUNCHES + CHOOSEACTION);
                        chooseParams = FightStatus.ChooseAction;
                        ui.GetInfo(StandartList, StandartList.Count);
                    }
                }           
            }
        }

        public void CheckStandartList()
        {
            if (answers.Count == 0)
            {
                StandartList.Remove("Поговорить");
            }
        }                    

        public void YourEnemyReaction()
        {
            Random rnd = new Random();
            Character whoIsOnDuty = enemies[rnd.Next(0, enemies.Count)];
            Artifact art;

            if (whoIsOnDuty.Inventory.Count != 0)
            {
                if (rnd.Next(0, 2) == 0)
                {
                    art = whoIsOnDuty.Inventory[rnd.Next(0, whoIsOnDuty.Inventory.Count)];
                    whoIsOnDuty.UseArtifact(art, parent.game.hero);
                    ABOUTENEMYPUNCHES = "Против вас использовали артефакт: " + art.NAME + "\nУдар нанес: " + whoIsOnDuty.Name + '\n';
                    ui.InfoAboutCurrentConditions(ABOUTENEMYPUNCHES);

                }
                else
                {
                    whoIsOnDuty.Hit(parent.game.hero);
                    ABOUTENEMYPUNCHES = "Вас ударили! Вы потеряли 15 баллов здоровья! \nОсталось: " + parent.game.hero.Health.ToString() + " \nУдар нанес: " + whoIsOnDuty.Name + '\n';
                    ui.InfoAboutCurrentConditions(ABOUTENEMYPUNCHES);
                }
            }
            else
            {
                whoIsOnDuty.Hit(parent.game.hero);
                ABOUTENEMYPUNCHES = "Вас ударили! Вы потеряли 15 баллов здоровья! \nОсталось: " + parent.game.hero.Health.ToString() + "\nУдар нанес: " + whoIsOnDuty.Name + '\n';
                ui.InfoAboutCurrentConditions(ABOUTENEMYPUNCHES);

            }
            ui.GetInfoCharacter(parent.game.hero);
            ui.GetInfoEnemies(enemies);
        }
    }
}