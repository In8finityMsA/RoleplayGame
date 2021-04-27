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
using KashTaskWPF.Artifacts;
using KashTaskWPF.Spells;
using static game.Character;
using static KashTaskWPF.Spells.Spell;

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
        private FightPlan plan;
        private Stager parent;

        private List<string> StandartList = new List<string>() { "Удар", "Заклинание", "Артефакт", "Поговорить", "Бежать" };

        private List<Character> enemiesPlusHero = new List<Character>();
        List<KeyValuePair<Type, Spell>> spells;
        List<Artifact> artifacts;    

        Stack<FightStatus> recorder = new Stack<FightStatus>();

        //in normal cases
        private string CHOOSETARGET = "Вы можете выбрать цель, на которую хотите направить свое действие!";
        private string CHOOSEACTION = "Выберите действие, которое хотите направить на врагов!";
        private string CHOOSEPOWER = "Выберите силу действия";
        private string CHOOSESPELL = "Выберите заклинание!";
        private string CHOOSEARTIFACT = "Выберите артефакт!";

        //in abnormal cases
        private string CHOOSEANOTHERACTION = "Выберите другое действие!";
        private string NOMANA = "У вас нет маны!";
        private string NOARTIFACTS = "У вас нет артефактов!";
        private string NOSPELLS = "У вас нет выученных заклинаний!";
        private string NOTENOUGHMANA = "Вам не хватило маны на заклинание!";
        private string YOUCANNOTEMOVEORTALK = "У вас плохо со здровьем. Вы либо не можете говорить, либо не можете двигаться. А для данного заклинания это важно.";
        private string PROBLEM = "";
        private string ENTER = "Ввести!";

        //dialog
        private string NOWORDS = "С вами не хотят говорить!";
        private string EXIT = "Закончить разговор";
        private string YOUDECIDEDTOINTERDIAL = "Вы решили прервать диалог!";
        private string YOUAREPARALIZEDCANNOTHIT = "Вы не можете двигаться, и не можете наносить удары.";
        private string CONVERSATION = "";

        List<string> words;
        List<string> answers;


        private string ABOUTENEMYPUNCHES = "";
        
        private MainWindow ui = KashTaskWPF.MainWindow.mainwindow;

        private FightAction whatNow;
        private FightStatus chooseParams = FightStatus.ChooseAction;

        private Character target;
        private Spell spell;
        private double power;
        private Artifact artifact;


        private bool flag = true;//

        public event OnStepActions StepHappened;

        public void SubscribeAllCharactersToStepHappend()
        {
            foreach (var item in enemiesPlusHero)
            {
                StepHappened += item.EventHandler;
            }    
        }

        public Fighter(Stager parent, FightPlan plan)
        {

            this.parent = parent;
            this.plan = plan;
            
            spells = ((Magician)parent.game.hero).Spells.ToList<KeyValuePair<Type, Spell>>();
            artifacts = parent.game.hero.Inventory;

            answers = plan.yourWord;
            words = plan.enemiesWord;
            enemiesPlusHero = plan.EnemyList;

            enemiesPlusHero.Add(parent.game.hero);

            //for ui
            ui.InfoAboutCurrentConditions(CHOOSEACTION);
            ui.GetInfo(StandartList, StandartList.Count);
            ui.GetInfoEnemies(enemiesPlusHero);
            ui.GetInfoCharacter(parent.game.hero);

            //for event
            SubscribeAllCharactersToStepHappend();         
        }

        public List<string> EnemyNamesToList()//only enemies, without hero
        {
            List<string> enemiesNames = new List<string>();

            for (int i = 0; i < enemiesPlusHero.Count - 1; i++)
            {
                enemiesNames.Add(enemiesPlusHero[i].Name);
            }
            enemiesNames.Add("На себя");
            return enemiesNames;
        }

        //public List<string> PowerToList()
        //{
        //    return new List<string>() { "10", "20", "30" , "40", "50"};
        //}

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

        public void InitNewRecorder()
        {
            recorder = new Stack<FightStatus>();
            recorder.Push(FightStatus.ChooseAction);
        }

        public void GetInput(int index)
        {

            if (chooseParams == FightStatus.ChooseAction)////// if gamer has to choose action
            {
                index += 1;
                whatNow = (FightAction)(index);

                InitNewRecorder();
                switch (index)
                {
                    case 1: //HIT
                        {
                            if (parent.game.hero.CanMoveNow)
                            {
                                if (enemiesPlusHero.Count > 1)
                                {
                                    chooseParams = FightStatus.ChooseTarget;
                                    recorder.Push(chooseParams);

                                    ui.InfoAboutCurrentConditions(CHOOSETARGET);
                                    ui.GetInfo(EnemyNamesToList(), enemiesPlusHero.Count);//providing gamer with options of enemies                   
                                }
                                else if (enemiesPlusHero.Count == 1)
                                {
                                    parent.EndFight(FightResult.WON);
                                }
                            }
                            else
                            {
                                
                                PROBLEM = YOUAREPARALIZEDCANNOTHIT;
                                ui.InfoAboutCurrentConditions(PROBLEM + '\n' + CHOOSEACTION);
                                PROBLEM = "";
                                chooseParams = FightStatus.ChooseAction;
                                ui.GetInfo(StandartList, StandartList.Count);
                                
                            }                                    
                        }                       
                        break;
                    case 2: //SPELL
                        {                   
                            if (spells.Count >= 1)
                            {                               
                                if (parent.game.hero.Mana > 0)
                                {                            
                                    chooseParams = FightStatus.ChooseSpell;
                                    recorder.Push(chooseParams);
                                    ui.InfoAboutCurrentConditions(CHOOSESPELL);
                                    ui.GetInfo(SpellNamesToList(), spells.Count);
                                }
                                else
                                {
                                   
                                    
                                    PROBLEM = NOMANA;
                                    ui.InfoAboutCurrentConditions(PROBLEM + '\n' + CHOOSEACTION);
                                    PROBLEM = "";
                                    chooseParams = FightStatus.ChooseAction;
                                    ui.GetInfo(StandartList, StandartList.Count);
                                   
                                }                              
                            }
                            else
                            {
                                                   
                                PROBLEM = NOSPELLS;
                                ui.InfoAboutCurrentConditions(PROBLEM + '\n' + CHOOSEACTION);
                                PROBLEM = "";
                                chooseParams = FightStatus.ChooseAction;
                                ui.GetInfo(StandartList, StandartList.Count);
                                
                            }                            
                        }
                        break;
                    case 3:  //ARTIFACT
                        {
                            if (artifacts.Count >= 1)
                            {
                                
                                ui.InfoAboutCurrentConditions(CHOOSEARTIFACT);
                                chooseParams = FightStatus.ChooseArtifact;
                                recorder.Push(chooseParams);
                                ui.GetInfo(ArtifactNamesToList(), artifacts.Count);
                            }
                            else
                            {
                                
                                PROBLEM = NOARTIFACTS;
                                ui.InfoAboutCurrentConditions(PROBLEM + '\n' + CHOOSEANOTHERACTION);
                                PROBLEM = "";
                                chooseParams = FightStatus.ChooseAction;
                                ui.GetInfo(StandartList, StandartList.Count);
                                
                            }
                        }
                        break;
                    case 4:  //TALK
                        {
                            if (words.Count == 0)
                            {
                                PROBLEM = NOWORDS;
                                ui.InfoAboutCurrentConditions(PROBLEM + '\n' + CHOOSEACTION);
                                PROBLEM = "";
                                chooseParams = FightStatus.ChooseAction;
                                ui.GetInfo(StandartList, StandartList.Count);
                            }
                            else
                            {

                                ui.InfoAboutCurrentConditions(CONVERSATION + words[0]);
                                ui.GetInfo(new List<string>() { answers[0], EXIT }, 2);
                                
                                chooseParams = FightStatus.ChooseWords;
                                recorder.Push(chooseParams);
                            }
                        }
                        break;
                    case 5: //RUN
                        {
                            parent.EndFight(FightResult.RAN);
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
                   
                    ui.InfoAboutCurrentConditions(CHOOSEPOWER);
                    chooseParams = FightStatus.ChoosePower;
                    recorder.Push(chooseParams);
                    ui.DisplayTextBox();
                    ui.GetInfo(new List<string>() { "Введите мощность" }, 1);
                }
                else
                {
                    if (enemiesPlusHero.Count > 1)
                    {
                     
                        ui.InfoAboutCurrentConditions(CHOOSETARGET);
                        chooseParams = FightStatus.ChooseTarget;
                        recorder.Push(chooseParams);
                        ui.GetInfo(EnemyNamesToList(), enemiesPlusHero.Count);
                    }
                    else if (enemiesPlusHero.Count == 1)
                    {
                        parent.EndFight(FightResult.WON);
                    }
                }
            }
            else if (chooseParams == FightStatus.ChooseArtifact)
            {
                artifact = artifacts[index];

                if (artifact is IMagicPowered)
                {
                 
                    ui.InfoAboutCurrentConditions(CHOOSEPOWER);
                    chooseParams = FightStatus.ChoosePower;
                    recorder.Push(chooseParams);
                    ui.DisplayTextBox();
                    ui.GetInfo(new List<string>() { "Введите мощность" }, 1);
                }
                else
                {
                    if (enemiesPlusHero.Count > 1)
                    {
                  
                        ui.InfoAboutCurrentConditions(CHOOSETARGET);
                        chooseParams = FightStatus.ChooseTarget;
                        recorder.Push(chooseParams);
                        ui.GetInfo(EnemyNamesToList(), enemiesPlusHero.Count);
                    }
                    else if (enemiesPlusHero.Count == 1)
                    {
                        parent.EndFight(FightResult.WON);
                    }
                }
            }
            else if (chooseParams == FightStatus.ChooseTarget)
            {
                target = enemiesPlusHero[index];

                if (whatNow == FightAction.HIT)
                {
                    parent.game.hero.Hit(target);

                    ui.GetInfoEnemies(enemiesPlusHero);
                    if (target.StateHealth == StateHealth.DEAD)
                    {
                        if (target == parent.game.hero)
                        {
                            parent.EndFight(FightResult.DIED);
                        }
                        else
                        {
                            StepHappened -= target.EventHandler;//unsubscribe
                            enemiesPlusHero.Remove(target);
                            ui.GetInfoEnemies(enemiesPlusHero);
                            //tell UI about murder?
                            if (enemiesPlusHero.Count == 1)
                            {
                                parent.EndFight(FightResult.WON);
                                return;
                            }
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


                    StepHappened();
                    ui.GetInfoEnemies(enemiesPlusHero);
                    ui.GetInfoCharacter(parent.game.hero);




                    recorder = new Stack<FightStatus>();
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
                            ((Magician)parent.game.hero).UseSpell(spell, target, power);//dffdfdf


                            ui.GetInfoCharacter(parent.game.hero);
                            ui.GetInfoEnemies(enemiesPlusHero);
                            if (target.StateHealth == StateHealth.DEAD)
                            {
                                if (target == parent.game.hero)
                                {
                                    parent.EndFight(FightResult.DIED);
                                }
                                else
                                {
                                    StepHappened -= target.EventHandler;
                                    enemiesPlusHero.Remove(target);
                                    ui.GetInfoEnemies(enemiesPlusHero);
                                    //tell UI about murder?
                                    if (enemiesPlusHero.Count == 1)
                                    {
                                        parent.EndFight(FightResult.WON);
                                        return;
                                    }
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


                            StepHappened();
                            ui.GetInfoEnemies(enemiesPlusHero);
                            ui.GetInfoCharacter(parent.game.hero);




                            recorder = new Stack<FightStatus>();
                            ui.InfoAboutCurrentConditions(ABOUTENEMYPUNCHES + CHOOSEACTION);
                            chooseParams = FightStatus.ChooseAction;
                            recorder.Push(chooseParams);
                            ui.GetInfo(StandartList, StandartList.Count);
                        }
                        catch (NotEnoughManaException)
                        {
                            
                            PROBLEM = NOTENOUGHMANA;
                            if (spells.Count > 1)
                            {
                                ui.InfoAboutCurrentConditions(PROBLEM + '\n' + CHOOSESPELL);           //working good
                                PROBLEM = "";        
                                InitNewRecorder();       //gooood       
                                chooseParams = FightStatus.ChooseSpell;
                                recorder.Push(chooseParams);    //gooood
                                ui.GetInfo(SpellNamesToList(), spells.Count);
                            }
                            else
                            {
                                ui.InfoAboutCurrentConditions(PROBLEM + '\n' + CHOOSEACTION);
                                PROBLEM = "";
                                chooseParams = FightStatus.ChooseAction;
                                InitNewRecorder();//test it, yes, we need to make new recorder       //work ok
                                ui.GetInfo(StandartList, StandartList.Count);
                            }
                            
                        }
                        catch (Exception)
                        {
                            
                            PROBLEM = YOUCANNOTEMOVEORTALK;
                            if (spells.Count > 1)
                            {
                                ui.InfoAboutCurrentConditions(PROBLEM + '\n' + CHOOSESPELL);
                                PROBLEM = "";
                                chooseParams = FightStatus.ChooseSpell;
                                //
                                InitNewRecorder();
                                recorder.Push(chooseParams);
                                ui.GetInfo(SpellNamesToList(), spells.Count);
                            }
                            else
                            {
                                ui.InfoAboutCurrentConditions(PROBLEM + '\n' + CHOOSEACTION);
                                PROBLEM = "";
                                chooseParams = FightStatus.ChooseAction;
                                InitNewRecorder();
                                ui.GetInfo(StandartList, StandartList.Count);
                            }
                            
                        }


                    }
                    else
                    {
                        try
                        {
                            ((Magician)parent.game.hero).UseSpell(spell, target);//ffdf



                            ui.GetInfoCharacter(parent.game.hero);
                            ui.GetInfoEnemies(enemiesPlusHero);
                            if (target.StateHealth == StateHealth.DEAD)
                            {
                                if (target == parent.game.hero)
                                {
                                    parent.EndFight(FightResult.DIED);
                                }
                                else 
                                {
                                    StepHappened -= target.EventHandler;//unsubscribe
                                    enemiesPlusHero.Remove(target);
                                    ui.GetInfoEnemies(enemiesPlusHero);
                                    //tell UI about murder?
                                    if (enemiesPlusHero.Count == 1)
                                    {
                                        parent.EndFight(FightResult.WON);
                                        return;
                                    }
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


                            StepHappened();
                            ui.GetInfoEnemies(enemiesPlusHero);
                            ui.GetInfoCharacter(parent.game.hero);




                            recorder = new Stack<FightStatus>();
                            ui.InfoAboutCurrentConditions(ABOUTENEMYPUNCHES + CHOOSEACTION);
                            chooseParams = FightStatus.ChooseAction;
                            recorder.Push(chooseParams);
                            ui.GetInfo(StandartList, StandartList.Count);

                        }
                        catch (NotEnoughManaException)
                        {
                            
                            PROBLEM = NOTENOUGHMANA;
                            if (spells.Count > 1)
                            {
                                ui.InfoAboutCurrentConditions(PROBLEM + '\n' + CHOOSESPELL);
                                PROBLEM = "";
                                chooseParams = FightStatus.ChooseSpell;
                                InitNewRecorder();
                                recorder.Push(chooseParams); 
                                ui.GetInfo(SpellNamesToList(), spells.Count);
                            }
                            else
                            {
                                ui.InfoAboutCurrentConditions(PROBLEM + '\n' + CHOOSEACTION);
                                PROBLEM = "";
                                chooseParams = FightStatus.ChooseAction;
                                InitNewRecorder();
                                ui.GetInfo(StandartList, StandartList.Count);
                            }
                            
                        }
                        catch (Exception)
                        {
                           
                            PROBLEM = YOUCANNOTEMOVEORTALK;
                            if (spells.Count > 1)
                            {
                                ui.InfoAboutCurrentConditions(PROBLEM + '\n' + CHOOSESPELL);
                                PROBLEM = "";
                                chooseParams = FightStatus.ChooseSpell;
                                InitNewRecorder();
                                recorder.Push(chooseParams);
                                ui.GetInfo(SpellNamesToList(), spells.Count);
                            }
                            else
                            {
                                ui.InfoAboutCurrentConditions(PROBLEM + '\n' + CHOOSEACTION);
                                PROBLEM = "";
                                InitNewRecorder();
                                chooseParams = FightStatus.ChooseAction;
                                ui.GetInfo(StandartList, StandartList.Count);
                            }
                            
                        }
                    }
                }
                else if (whatNow == FightAction.ARTIFACT)
                {
                    if (artifact is IMagicPowered)
                    {
                        parent.game.hero.UseArtifact((PoweredRenewableArtifact)artifact, target, power);
                        InitNewRecorder();

                        ui.GetInfoEnemies(enemiesPlusHero);
                        if (target.StateHealth == StateHealth.DEAD)
                        {
                            if (target == parent.game.hero)
                            {
                                parent.EndFight(FightResult.DIED);
                            }
                            else
                            {
                                StepHappened -= target.EventHandler;//unsubscribe
                                enemiesPlusHero.Remove(target);
                                ui.GetInfoEnemies(enemiesPlusHero);
                                //tell UI about murder?
                                if (enemiesPlusHero.Count == 1)
                                {
                                    parent.EndFight(FightResult.WON);
                                    return;
                                }
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


                        StepHappened();
                        ui.GetInfoEnemies(enemiesPlusHero);
                        ui.GetInfoCharacter(parent.game.hero);




                        recorder = new Stack<FightStatus>();
                        ui.InfoAboutCurrentConditions(ABOUTENEMYPUNCHES + CHOOSEACTION);
                        chooseParams = FightStatus.ChooseAction;
                        recorder.Push(chooseParams);
                        ui.GetInfo(StandartList, StandartList.Count);
                    }
                    else
                    {


                        parent.game.hero.UseArtifact(artifact, target);



                        ui.GetInfoEnemies(enemiesPlusHero);
                        if (target.StateHealth == StateHealth.DEAD)
                        {
                            if (target == parent.game.hero)
                            {
                                parent.EndFight(FightResult.DIED);
                            }
                            else
                            {
                                StepHappened -= target.EventHandler;//unsubscribe
                                enemiesPlusHero.Remove(target);
                                ui.GetInfoEnemies(enemiesPlusHero);
                                //tell UI about murder?
                                if (enemiesPlusHero.Count == 1)
                                {
                                    parent.EndFight(FightResult.WON);
                                    return;
                                }
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



                        StepHappened();
                        ui.GetInfoEnemies(enemiesPlusHero);
                        ui.GetInfoCharacter(parent.game.hero);





                        recorder = new Stack<FightStatus>();
                        ui.InfoAboutCurrentConditions(ABOUTENEMYPUNCHES + CHOOSEACTION);
                        chooseParams = FightStatus.ChooseAction;
                        recorder.Push(chooseParams);
                        ui.GetInfo(StandartList, StandartList.Count);
                    }
                }                         
            }
            else if (chooseParams == FightStatus.ChooseWords)//CHOOSEWORDS
            {
                index += 1;
                switch (index)
                {
                    case 1:
                        {
                            CONVERSATION += (words[0] + '\n' + answers[0] + '\n');
                            ui.InfoAboutCurrentConditions(CONVERSATION);
                            words.RemoveAt(0);
                            answers.RemoveAt(0);
                            if (words.Count == 0)
                            {
                                chooseParams = FightStatus.ChooseAction;
                                ui.InfoAboutCurrentConditions(CONVERSATION + '\n' + NOWORDS + '\n' + CHOOSEACTION);
                                ui.GetInfo(StandartList, StandartList.Count);
                            }
                            else
                            {
                                chooseParams = FightStatus.ChooseWords;
                                ui.InfoAboutCurrentConditions(CONVERSATION + words[0]);
                                ui.GetInfo(new List<string>() { answers[0], EXIT }, 2);
                                InitNewRecorder();
                            }
                            break;
                        }

                    case 2:
                        {
                            PROBLEM = YOUDECIDEDTOINTERDIAL;
                            ui.InfoAboutCurrentConditions(PROBLEM + '\n' + CHOOSEACTION);
                            PROBLEM = "";
                            chooseParams = FightStatus.ChooseAction;
                            ui.GetInfo(StandartList, StandartList.Count);
                            break;
                        }
                }
            }
            else if (chooseParams == FightStatus.ChoosePower)//CHOOSEPOWER
            {
                
                try
                {
                    power = Convert.ToInt32(ui.GetUserInputText());

                    ui.HideTextBox();

                    if (enemiesPlusHero.Count > 1)
                    {

                        ui.InfoAboutCurrentConditions(CHOOSETARGET);
                        chooseParams = FightStatus.ChooseTarget;
                        recorder.Push(chooseParams);
                        ui.GetInfo(EnemyNamesToList(), enemiesPlusHero.Count);

                    }
                    else if (enemiesPlusHero.Count == 1)
                    {
                        parent.EndFight(FightResult.WON);
                    }
                }
                catch
                {
                    ui.InfoAboutCurrentConditions("Упс. Что-то пошло не так...");
                }      
            }
        }

        public void GivePrevStep()
        {
            ui.HideTextBox();
            if (recorder.Count > 1)
            {
                recorder.Pop();
                chooseParams = recorder.Peek();

                DrawSituation(chooseParams);
            }
        }

        public void DrawSituation(FightStatus status)
        {
            switch (status)
            {
                case FightStatus.ChooseAction:
                    {
                        ui.GetInfo(StandartList, StandartList.Count);
                        ui.GetInfoEnemies(enemiesPlusHero);
                        ui.GetInfoCharacter(parent.game.hero);
                        ui.InfoAboutCurrentConditions(CHOOSEACTION);
                    }
                    break;
                case FightStatus.ChooseTarget:
                    {
                        ui.InfoAboutCurrentConditions(CHOOSETARGET);
                        ui.GetInfo(EnemyNamesToList(), enemiesPlusHero.Count);
                        ui.GetInfoEnemies(enemiesPlusHero);
                        ui.GetInfoCharacter(parent.game.hero);
                    }
                    break;
                case FightStatus.ChoosePower:
                    {
                        ui.InfoAboutCurrentConditions(CHOOSEPOWER);
                        ui.DisplayTextBox();
                        ui.GetInfo(new List<string>() { "Введите мощность" }, 1);
                        ui.GetInfoEnemies(enemiesPlusHero);
                        ui.GetInfoCharacter(parent.game.hero);
                    }
                    break;
                case FightStatus.ChooseSpell:
                    {
                        ui.InfoAboutCurrentConditions(CHOOSESPELL);
                        ui.GetInfo(SpellNamesToList(), spells.Count);
                        ui.GetInfoEnemies(enemiesPlusHero);
                        ui.GetInfoCharacter(parent.game.hero);
                    }
                    break;
                case FightStatus.ChooseArtifact:
                    {
                        ui.InfoAboutCurrentConditions(CHOOSEARTIFACT);
                        ui.GetInfo(ArtifactNamesToList(), artifacts.Count);
                        ui.GetInfoEnemies(enemiesPlusHero);
                        ui.GetInfoCharacter(parent.game.hero);
                    }
                    break;
                case FightStatus.ChooseWords:
                    break;
                default:
                    break;
            }
        }

        public void YourEnemyReaction()
        {
            Random rnd = new Random();
            Character whoIsOnDuty = enemiesPlusHero[rnd.Next(0, enemiesPlusHero.Count - 1)];// -1 not to kill yourself
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
                    if (!parent.game.hero.StatesDynamic.ContainsKey(State.ARMOR))
                    {
                        ABOUTENEMYPUNCHES = "Вас ударили! Вы потеряли 15 баллов здоровья! \nОсталось: " + parent.game.hero.Health.ToString() + " \nУдар нанес: " + whoIsOnDuty.Name + '\n';
                    }
                    else
                    {
                        ABOUTENEMYPUNCHES = "Вас хотели ударить, но у вас броня! " + " \nУдар пытался нанести: " + whoIsOnDuty.Name + '\n';
                    }
                    ui.InfoAboutCurrentConditions(ABOUTENEMYPUNCHES);
                }
            }
            else
            {
                whoIsOnDuty.Hit(parent.game.hero);
                if (!parent.game.hero.StatesDynamic.ContainsKey(State.ARMOR))
                {
                    ABOUTENEMYPUNCHES = "Вас ударили! Вы потеряли 15 баллов здоровья! \nОсталось: " + parent.game.hero.Health.ToString() + "\nУдар нанес: " + whoIsOnDuty.Name + '\n';
                }
                else
                {
                    ABOUTENEMYPUNCHES = "Вас хотели ударить, но у вас броня! " + " \nУдар пытался нанести: " + whoIsOnDuty.Name + '\n';
                }
                ui.InfoAboutCurrentConditions(ABOUTENEMYPUNCHES);
            }
            ui.GetInfoCharacter(parent.game.hero);
            ui.GetInfoEnemies(enemiesPlusHero);
        }
    }
}