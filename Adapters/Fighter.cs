﻿using game;
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
        //private readonly string fileName;
        private FightPlan plan;

        private List<string> StandartList = new List<string>() { "Удар", "Заклинание", "Артефакт", "Поговорить", "Бежать" };
        private List<Character> enemies = new List<Character>();
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


        private string YOUAREPARALIZEDCANNOTHIT = "Вы не можете двигаться, и не можете наносить удары.";
        private string CONVERSATION = "";
        List<string> words;
        List<string> answers;


        private string ABOUTENEMYPUNCHES = "";
        private Stager parent;
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
            foreach (var item in enemies)
            {
                StepHappened += item.EventHandler;
            }    
        }

        public Fighter(/*string fileName,*/ Stager parent, FightPlan plan)
        {
            //this.fileName = fileName;
            //JsonInit();

            this.parent = parent;
            this.plan = plan;
            
            spells = ((Magician)parent.game.hero).Spells.ToList<KeyValuePair<Type, Spell>>();
            artifacts = parent.game.hero.Inventory;

            words = plan.yourWord;
            answers = plan.enemiesWord;

            JoinLists();
            enemies.Add(parent.game.hero);

            //for ui
            ui.InfoAboutCurrentConditions("Вы можете выбрать действие, чтобы атаковать врага!");
            ui.GetInfo(StandartList, StandartList.Count);
            ui.GetInfoEnemies(enemies);
            ui.GetInfoCharacter(parent.game.hero);

            //for event
            SubscribeAllCharactersToStepHappend();         
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

            for (int i = 0; i < enemies.Count - 1; i++)
            {
                enemiesNames.Add(enemies[i].Name);
            }
            enemiesNames.Add("На себя");
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

                //if (flag)
                //{
                //    StepHappened();
                //    ui.GetInfoEnemies(enemies);
                //    ui.GetInfoCharacter(parent.game.hero);
                //}
                //else
                //{
                //    flag = true;
                //}

                InitNewRecorder();
                switch (index)
                {
                    case 1: //HIT
                        {
                            if (parent.game.hero.CanMoveNow)
                            {
                                if (enemies.Count > 1)
                                {
                                    chooseParams = FightStatus.ChooseTarget;
                                    recorder.Push(chooseParams);

                                    ui.InfoAboutCurrentConditions(CHOOSETARGET);
                                    ui.GetInfo(EnemyNamesToList(), enemies.Count);//providing gamer with options of enemies                   
                                }
                                else if (enemies.Count == 1)
                                {
                                    parent.game.hero.Hit(enemies[0]);


                                    if (enemies[0].StateHealth == StateHealth.DEAD)
                                    {
                                        StepHappened -= enemies[0].EventHandler;//unsubscribe
                                        enemies.RemoveAt(0);
                                        ui.GetInfoEnemies(enemies);// empty list
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




                                    StepHappened();
                                    ui.GetInfoEnemies(enemies);
                                    ui.GetInfoCharacter(parent.game.hero);




                                    //StepHappened();
                                    recorder = new Stack<FightStatus>();
                                    ui.InfoAboutCurrentConditions(ABOUTENEMYPUNCHES + CHOOSEACTION);
                                    chooseParams = FightStatus.ChooseAction;
                                    recorder.Push(chooseParams);
                                    ui.GetInfo(StandartList, StandartList.Count);
                                }
                            }
                            else
                            {
                                //
                                //
                                //
                                //
                                PROBLEM = YOUAREPARALIZEDCANNOTHIT;
                                ui.InfoAboutCurrentConditions(PROBLEM + '\n' + CHOOSEACTION);
                                PROBLEM = "";
                                chooseParams = FightStatus.ChooseAction;
                                ui.GetInfo(StandartList, StandartList.Count);
                                //
                                //
                                //
                                //
                            }                                    
                        }                       
                        break;
                    case 2: //SPELL
                        {
                            //spells = new List<KeyValuePair<Type, Spell>>();                      
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
                                    //
                                    //
                                    //
                                    //
                                    
                                    PROBLEM = NOMANA;
                                    ui.InfoAboutCurrentConditions(PROBLEM + '\n' + CHOOSEACTION);
                                    PROBLEM = "";
                                    chooseParams = FightStatus.ChooseAction;
                                    ui.GetInfo(StandartList, StandartList.Count);
                                    //
                                    //
                                    //
                                    //
                                }                              
                            }
                            else
                            {
                                //
                                //
                                //
                                //
                                //                                
                                PROBLEM = NOSPELLS;
                                ui.InfoAboutCurrentConditions(PROBLEM + '\n' + CHOOSEACTION);
                                PROBLEM = "";
                                chooseParams = FightStatus.ChooseAction;
                                ui.GetInfo(StandartList, StandartList.Count);
                                //
                                //
                                //
                                //
                                //
                            }                            
                        }
                        break;
                    case 3:  //ARTIFACT
                        {
                            //artifacts = new List<Artifact>();
                            if (artifacts.Count >= 1)
                            {
                                
                                ui.InfoAboutCurrentConditions(CHOOSEARTIFACT);
                                chooseParams = FightStatus.ChooseArtifact;
                                recorder.Push(chooseParams);
                                ui.GetInfo(ArtifactNamesToList(), artifacts.Count);
                            }
                            else
                            {
                                //
                                //
                                //
                                //
                                //
                                //ui.InfoAboutCurrentConditions(NOARTIFACTS + '\n' + CHOOSEANOTHERACTION);
                                PROBLEM = NOARTIFACTS;
                                ui.InfoAboutCurrentConditions(PROBLEM + '\n' + CHOOSEANOTHERACTION);
                                PROBLEM = "";
                                chooseParams = FightStatus.ChooseAction;
                                ui.GetInfo(StandartList, StandartList.Count);
                                //
                                //
                                //
                                //
                                //
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
                    ui.GetInfo(PowerToList(), 5);
                }
                else
                {
                    if (enemies.Count != 1)
                    {
                     
                        ui.InfoAboutCurrentConditions(CHOOSETARGET);
                        chooseParams = FightStatus.ChooseTarget;
                        recorder.Push(chooseParams);
                        ui.GetInfo(EnemyNamesToList(), enemies.Count);
                    }
                    else if (enemies.Count == 1)
                    {
                        target = enemies[0];
                        try
                        {
                            ((Magician)parent.game.hero).UseSpell(spell, target);///dgdgfdfg

                            ui.GetInfoEnemies(enemies);
                            ui.GetInfoCharacter(parent.game.hero);
                            if (enemies[0].StateHealth == StateHealth.DEAD)
                            {
                                StepHappened -= enemies[0].EventHandler;
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

                            StepHappened();
                            ui.GetInfoEnemies(enemies);
                            ui.GetInfoCharacter(parent.game.hero);





                            recorder = new Stack<FightStatus>();
                            ui.InfoAboutCurrentConditions(ABOUTENEMYPUNCHES + CHOOSEACTION);
                            chooseParams = FightStatus.ChooseAction;
                            recorder.Push(chooseParams);
                            ui.GetInfo(StandartList, StandartList.Count);

                        }
                        catch (NotEnoughManaException)
                        {
                            //
                            //
                            //
                            //
                            //ui.InfoAboutCurrentConditions(NOTENOUGHMANA + '\n' + CHOOSEACTION);
                            //chooseParams = FightStatus.ChooseAction;
                            //recorder.Push(chooseParams);
                            //ui.GetInfo(StandartList, StandartList.Count);

                            PROBLEM = NOTENOUGHMANA;
                            if (spells.Count > 1)
                            {
                                ui.InfoAboutCurrentConditions(PROBLEM + '\n' + CHOOSESPELL);
                                PROBLEM = "";
                                chooseParams = FightStatus.ChooseSpell;
                                InitNewRecorder();///??????
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
                            //
                            //
                            //
                            //
                            //
                        }
                        catch (Exception)
                        {
                            //
                            //
                            //
                            //
                            //
                            PROBLEM = YOUCANNOTEMOVEORTALK;
                            if (spells.Count > 1)
                            {
                                ui.InfoAboutCurrentConditions(PROBLEM + '\n' + CHOOSESPELL);
                                PROBLEM = "";
                                chooseParams = FightStatus.ChooseSpell;
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
                            //
                            //
                            //
                            //
                            //
                        }                                   
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
                    ui.GetInfo(PowerToList(), 5);
                }
                else
                {
                    if (enemies.Count >= 1)
                    {
                  
                        ui.InfoAboutCurrentConditions(CHOOSETARGET);
                        chooseParams = FightStatus.ChooseTarget;
                        recorder.Push(chooseParams);
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
                            recorder.Push(chooseParams);
                            ui.GetInfo(StandartList, StandartList.Count);
                        }
                        
                        ui.GetInfoEnemies(enemies);
                        if (enemies[0].StateHealth == StateHealth.DEAD)
                        {
                            StepHappened -= enemies[0].EventHandler;//unsubscribe
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


                        StepHappened();
                        ui.GetInfoEnemies(enemies);
                        ui.GetInfoCharacter(parent.game.hero);




                        recorder = new Stack<FightStatus>();
                        ui.InfoAboutCurrentConditions(ABOUTENEMYPUNCHES + CHOOSEACTION);
                        chooseParams = FightStatus.ChooseAction;
                        recorder.Push(chooseParams);
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
                        StepHappened -= target.EventHandler;//unsubscribe
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


                    StepHappened();
                    ui.GetInfoEnemies(enemies);
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
                            ui.GetInfoEnemies(enemies);
                            if (target.StateHealth == StateHealth.DEAD)
                            {
                                StepHappened -= target.EventHandler;
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


                            StepHappened();
                            ui.GetInfoEnemies(enemies);
                            ui.GetInfoCharacter(parent.game.hero);




                            recorder = new Stack<FightStatus>();
                            ui.InfoAboutCurrentConditions(ABOUTENEMYPUNCHES + CHOOSEACTION);
                            chooseParams = FightStatus.ChooseAction;
                            recorder.Push(chooseParams);
                            ui.GetInfo(StandartList, StandartList.Count);
                        }
                        catch (NotEnoughManaException)
                        {
                            //
                            //
                            //
                            //
                            //ui.InfoAboutCurrentConditions(NOTENOUGHMANA + '\n' + CHOOSEACTION);
                            //chooseParams = FightStatus.ChooseAction;
                            //recorder.Push(chooseParams);
                            //ui.GetInfo(StandartList, StandartList.Count);

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
                            //
                            //
                            //
                            //
                            //
                        }
                        catch (Exception)
                        {
                            //
                            //
                            //
                            //
                            //
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
                            //
                            //
                            //
                            //
                            //
                        }


                    }
                    else
                    {
                        try
                        {
                            ((Magician)parent.game.hero).UseSpell(spell, target);//ffdf



                            ui.GetInfoCharacter(parent.game.hero);
                            ui.GetInfoEnemies(enemies);
                            if (target.StateHealth == StateHealth.DEAD)
                            {
                                StepHappened -= target.EventHandler;//unsubscribe
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


                            StepHappened();
                            ui.GetInfoEnemies(enemies);
                            ui.GetInfoCharacter(parent.game.hero);




                            recorder = new Stack<FightStatus>();
                            ui.InfoAboutCurrentConditions(ABOUTENEMYPUNCHES + CHOOSEACTION);
                            chooseParams = FightStatus.ChooseAction;
                            recorder.Push(chooseParams);
                            ui.GetInfo(StandartList, StandartList.Count);

                        }
                        catch (NotEnoughManaException)
                        {
                            //
                            //
                            //
                            //
                            //ui.InfoAboutCurrentConditions(NOTENOUGHMANA + '\n' + CHOOSEACTION);
                            //chooseParams = FightStatus.ChooseAction;
                            //recorder.Push(chooseParams);
                            //ui.GetInfo(StandartList, StandartList.Count);

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
                            //
                            //
                            //
                            //
                            //
                        }
                        catch (Exception)
                        {
                            //
                            //
                            //
                            //
                            //
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
                            //
                            //
                            //
                            //
                            //
                        }
                    }
                }
                else if (whatNow == FightAction.ARTIFACT)
                {
                    if (artifact is IMagicPowered)
                    {
                        parent.game.hero.UseArtifact((PoweredRenewableArtifact)artifact, target, power);
                        InitNewRecorder();

                        ui.GetInfoEnemies(enemies);
                        if (target.StateHealth == StateHealth.DEAD)
                        {
                            StepHappened -= target.EventHandler;//unsubscribe
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


                        StepHappened();
                        ui.GetInfoEnemies(enemies);
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



                        ui.GetInfoEnemies(enemies);
                        if (target.StateHealth == StateHealth.DEAD)
                        {
                            StepHappened -= target.EventHandler;//unsubscribe
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



                        StepHappened();
                        ui.GetInfoEnemies(enemies);
                        ui.GetInfoCharacter(parent.game.hero);





                        recorder = new Stack<FightStatus>();
                        ui.InfoAboutCurrentConditions(ABOUTENEMYPUNCHES + CHOOSEACTION);
                        chooseParams = FightStatus.ChooseAction;
                        recorder.Push(chooseParams);
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

                    ui.InfoAboutCurrentConditions(CHOOSETARGET);
                    chooseParams = FightStatus.ChooseTarget;
                    recorder.Push(chooseParams);
                    ui.GetInfo(EnemyNamesToList(), enemies.Count);

                }
                else if (enemies.Count == 1)
                {
                    if (whatNow == FightAction.SPELL)
                    {
                        try
                        {
                            ((Magician)parent.game.hero).UseSpell(spell, enemies[0], power);//dgggfg





                            ui.GetInfoCharacter(parent.game.hero);
                            ui.GetInfoEnemies(enemies);
                            if (target.StateHealth == StateHealth.DEAD)
                            {
                                StepHappened -= target.EventHandler;//unsubscribe
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


                            StepHappened();
                            ui.GetInfoEnemies(enemies);
                            ui.GetInfoCharacter(parent.game.hero);





                            recorder = new Stack<FightStatus>();
                            ui.InfoAboutCurrentConditions(ABOUTENEMYPUNCHES + CHOOSEACTION);
                            chooseParams = FightStatus.ChooseAction;
                            recorder.Push(chooseParams);
                            ui.GetInfo(StandartList, StandartList.Count);


                        }
                        //catch (Exception)
                        //{
                        //    ui.InfoAboutCurrentConditions(NOTENOUGHMANA + '\n' + CHOOSEACTION);
                        //    chooseParams = FightStatus.ChooseAction;
                        //    recorder.Push(chooseParams);
                        //    ui.GetInfo(StandartList, StandartList.Count);
                        //}
                        catch (NotEnoughManaException)
                        {
                            //
                            //
                            //
                            //
                            //ui.InfoAboutCurrentConditions(NOTENOUGHMANA + '\n' + CHOOSEACTION);
                            //chooseParams = FightStatus.ChooseAction;
                            //recorder.Push(chooseParams);
                            //ui.GetInfo(StandartList, StandartList.Count);

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
                                InitNewRecorder();
                                chooseParams = FightStatus.ChooseAction;
                                ui.GetInfo(StandartList, StandartList.Count);
                            }
                            //
                            //
                            //
                            //
                            //
                        }
                        catch (Exception)
                        {
                            //
                            //
                            //
                            //
                            //
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
                            //
                            //
                            //
                            //
                            //
                        }

                    }
                    else if (whatNow == FightAction.ARTIFACT)
                    {
                        parent.game.hero.UseArtifact((PoweredRenewableArtifact)artifact, enemies[0], power);




                        ui.GetInfoEnemies(enemies);
                        
                        if (target.StateHealth == StateHealth.DEAD)
                        {
                            StepHappened -= target.EventHandler;//unsubscribe
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

                        StepHappened();
                        ui.GetInfoEnemies(enemies);
                        ui.GetInfoCharacter(parent.game.hero);




                        recorder = new Stack<FightStatus>();
                        ui.InfoAboutCurrentConditions(ABOUTENEMYPUNCHES + CHOOSEACTION);
                        chooseParams = FightStatus.ChooseAction;
                        recorder.Push(chooseParams);
                        ui.GetInfo(StandartList, StandartList.Count);
                    }
                }           
            }
        }

        public void GivePrevStep()
        {
            if (recorder.Count > 1)
            {
                recorder.Pop();
                chooseParams = recorder.Peek();

                if (chooseParams == FightStatus.ChooseAction)
                {
                    flag = false;
                }
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
                        ui.GetInfoEnemies(enemies);
                        ui.GetInfoCharacter(parent.game.hero);
                        ui.InfoAboutCurrentConditions(CHOOSEACTION);
                    }
                    break;
                case FightStatus.ChooseTarget:
                    {
                        ui.InfoAboutCurrentConditions(CHOOSETARGET);
                        ui.GetInfo(EnemyNamesToList(), enemies.Count);
                        ui.GetInfoEnemies(enemies);
                        ui.GetInfoCharacter(parent.game.hero);
                    }
                    break;
                case FightStatus.ChoosePower:
                    {
                        ui.InfoAboutCurrentConditions(CHOOSEPOWER);
                        ui.GetInfo(PowerToList(), 5);
                        ui.GetInfoEnemies(enemies);
                        ui.GetInfoCharacter(parent.game.hero);
                    }
                    break;
                case FightStatus.ChooseSpell:
                    {
                        ui.InfoAboutCurrentConditions(CHOOSESPELL);
                        ui.GetInfo(SpellNamesToList(), spells.Count);
                        ui.GetInfoEnemies(enemies);
                        ui.GetInfoCharacter(parent.game.hero);
                    }
                    break;
                case FightStatus.ChooseArtifact:
                    {
                        ui.InfoAboutCurrentConditions(CHOOSEARTIFACT);
                        ui.GetInfo(ArtifactNamesToList(), artifacts.Count);
                        ui.GetInfoEnemies(enemies);
                        ui.GetInfoCharacter(parent.game.hero);
                    }
                    break;
                case FightStatus.ChooseWords:
                    break;
                default:
                    break;
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
            Character whoIsOnDuty = enemies[rnd.Next(0, enemies.Count - 1)];// -1 not to kill yourself
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