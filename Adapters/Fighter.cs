﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using static KashTaskWPF.Character;
using static KashTaskWPF.Spells.Spell;
using System.Windows;
using KashTaskWPF.Interface;

namespace KashTaskWPF.Adapters
{
    public enum FightResult
    {
        WON, DIED, RAN, NEGOTIATED
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
        private List<KeyValuePair<Type, Spell>> spells;
        private List<Artifact> artifacts;
        private List<string> variants = new List<string>();

        private Stack<FightStatus> recorder = new Stack<FightStatus>();

        //in normal cases
        private string CHOOSETARGET = "Вы можете выбрать цель, на которую хотите направить свое действие!";
        private string CHOOSEACTION = "Выберите действие, которое хотите направить на врагов!";
        private string CHOOSEPOWER = "Выберите силу действия. Если вы введете слишком большое число, а у вас нет таких мощностей, то будет использован весь ваш заряд.";
        private string CHOOSESPELL = "Выберите заклинание!";
        private string CHOOSEARTIFACT = "Выберите артефакт!";
        private string USEATYOURSELF = "На себя";
        private string ENEMYISPARALIZED = "Ваш противник парализован и не может нанести удар";

        //in abnormal cases
        private string CHOOSEANOTHERACTION = "Выберите другое действие!";
        private string NOMANA = "У вас нет маны!";
        private string NOARTIFACTS = "У вас нет артефактов!";
        private string NOSPELLS = "У вас нет выученных заклинаний!";
        private string NOTENOUGHMANA = "Вам не хватило маны на заклинание!";
        private string YOUCANNOTEMOVEORTALK = "У вас плохо со здровьем. Вы либо не можете говорить, либо не можете двигаться. А для данного заклинания это важно.";
        private string PROBLEM = "";
        private string ENTER = "Ввести!";
        private string POWERRANOUT = "У этого артефакта закончилась мощность!";
        private string SOMETHINGWENTWRONG = "Упс. Что-то пошло не так...";
        private string YOUCANNOTSPEAK = "Вы не можете говорить в данный момент.";

        //dialog
        private string NOWORDS = "С вами не хотят говорить!";
        private string EXIT = "Закончить разговор";
        private string YOUDECIDEDTOINTERDIAL = "Вы решили прервать диалог!";
        private string YOUAREPARALIZEDCANNOTHIT = "Вы не можете двигаться, и не можете наносить удары.";
        private string CONVERSATION = "";
        private string YOUAREKIND = "Вы подружились с гидрой и она пообещала снять заклятие!";

        //enemy punches
        private string THEYUSEDARTIFACT = "Против вас использовали артефакт:";
        private string THEYWANTEDHITBUTARMOR = "Вас хотели ударить, но у вас броня!";
        private string WHOUNSUCCEDHIT = "Удар пытался нанести: ";
        private string WHOHITED = "Удар нанес:";
        private string THEYMANAGEDTOHIT = "Вас ударили!";
        private string YOULOSTHEALTHPOINTS = "Вы потеряли баллов здоровья:";



        List<string> words;
        List<List<string>> answers;


        private string ABOUTENEMYPUNCHES = "";

        private MainWindow ui;

        private FightAction whatNow;
        private FightStatus chooseParams = FightStatus.ChooseAction;

        private Character target;
        private Spell spell;
        private double power;
        private Artifact artifact;

        public event OnStepActions StepHappened;

        private void SubscribeAllCharactersToStepHappend()
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
            this.ui = parent.ui;
            
            spells = ((Magician)parent.hero).Spells.ToList<KeyValuePair<Type, Spell>>();
            artifacts = parent.hero.Inventory;

            answers = plan.yourWord;
            words = plan.enemiesWord;
            enemiesPlusHero = plan.EnemyList;

            enemiesPlusHero.Add(parent.hero);

            //for ui
            ui.InfoAboutCurrentConditions(CHOOSEACTION);
            ui.GetInfo(StandartList, StandartList.Count);
            ui.GetInfoEnemies(enemiesPlusHero);
            ui.GetInfoCharacter(parent.hero);

            //for event
            SubscribeAllCharactersToStepHappend();         
        }

        private List<string> EnemyNamesToList()//only enemies, without hero
        {
            List<string> enemiesNames = new List<string>();

            for (int i = 0; i < enemiesPlusHero.Count - 1; i++)
            {
                enemiesNames.Add(enemiesPlusHero[i].Name);
            }
            enemiesNames.Add(USEATYOURSELF);
            return enemiesNames;
        }

        private List<string> SpellNamesToList()
        {
            List<string> spellsNames = new List<string>();
            for (int i = 0; i < spells.Count; i++)
            {
                spellsNames.Add(spells[i].Value.NAME);
            }
            return spellsNames;
        }

        private List<string> ArtifactNamesToList()
        {
            artifacts = parent.hero.Inventory;
            List<string> artifactNames = new List<string>();
            for (int i = 0; i < artifacts.Count; i++)
            {
                artifactNames.Add(artifacts[i].NAME);
            }
            return artifactNames;
        }

        private void InitNewRecorder()
        {
            recorder = new Stack<FightStatus>();
            recorder.Push(FightStatus.ChooseAction);
        }
        private void RememberToComeBack()
        {
            recorder.Push(chooseParams);
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
                            if (parent.hero.CanMoveNow)
                            {
                                if (enemiesPlusHero.Count > 1)
                                {
                                    chooseParams = FightStatus.ChooseTarget;
                                    RememberToComeBack();

                                    DrawSpecificSituation(CHOOSETARGET, EnemyNamesToList());                  
                                }
                                else if (enemiesPlusHero.Count == 1)
                                {
                                    parent.hero.Experience += plan.EXP;
                                    parent.EndFight(FightResult.WON);
                                }
                            }
                            else
                            {
                                DrawSpecificSituation(YOUAREPARALIZEDCANNOTHIT + '\n' + CHOOSEACTION, StandartList);                                
                                chooseParams = FightStatus.ChooseAction;                                
                            }                                    
                        }                       
                        break;
                    case 2: //SPELL
                        {                   
                            if (spells.Count >= 1)
                            {                               
                                if (parent.hero.Mana > 0)
                                {                            
                                    chooseParams = FightStatus.ChooseSpell;
                                    RememberToComeBack();

                                    DrawSpecificSituation(CHOOSESPELL, SpellNamesToList());
                                }
                                else
                                {
                                    DrawSpecificSituation(NOMANA + '\n' + CHOOSEACTION, StandartList);                                    
                                    chooseParams = FightStatus.ChooseAction;                                   
                                }                              
                            }
                            else
                            {
                                DrawSpecificSituation(NOSPELLS + '\n' + CHOOSEACTION, StandartList);
                                chooseParams = FightStatus.ChooseAction;
                            }                            
                        }
                        break;
                    case 3:  //ARTIFACT
                        {
                            if (artifacts.Count >= 1)
                            {
                                chooseParams = FightStatus.ChooseArtifact;
                                RememberToComeBack();

                                DrawSpecificSituation(CHOOSEARTIFACT, ArtifactNamesToList());
                            }
                            else
                            {
                                DrawSpecificSituation(NOARTIFACTS + '\n' + CHOOSEANOTHERACTION, StandartList);                               
                                chooseParams = FightStatus.ChooseAction;                               
                            }
                        }
                        break;
                    case 4:  //TALK
                        {

                            if (words.Count == 0)
                            {
                                chooseParams = FightStatus.ChooseAction;
                                DrawSpecificSituation(NOWORDS + '\n' + CHOOSEACTION, StandartList);                                                             
                            }
                            else if (parent.hero.CanSpeakNow == false)
                            {
                                chooseParams = FightStatus.ChooseAction;
                                DrawSpecificSituation(YOUCANNOTSPEAK + '\n' + CHOOSEACTION, StandartList);
                            }
                            else
                            {
                                variants = new List<string>(answers[0]);
                                variants.Add(EXIT);
                                DrawSpecificSituation(CONVERSATION + words[0], variants);

                                chooseParams = FightStatus.ChooseWords;
                                RememberToComeBack();
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
            else if (chooseParams == FightStatus.ChooseSpell)//CHOOSESPELL
            {
                spell = spells[index].Value;
                if (spell is IMagicPowered)
                {                  
                    chooseParams = FightStatus.ChoosePower;
                    RememberToComeBack();

                    ui.DisplayTextBox();
                    DrawSpecificSituation(CHOOSEPOWER, new List<string>() { ENTER });
                }
                else
                {
                    if (enemiesPlusHero.Count > 1)
                    {
                        DrawSpecificSituation(CHOOSETARGET, EnemyNamesToList());

                        chooseParams = FightStatus.ChooseTarget;
                        RememberToComeBack();

                    }
                    else if (enemiesPlusHero.Count == 1)
                    {
                        parent.hero.Experience += plan.EXP;
                        parent.EndFight(FightResult.WON);
                    }
                }
            }
            else if (chooseParams == FightStatus.ChooseArtifact)//CHOOSEARTIFACT
            {
                artifact = artifacts[index];

                if (artifact is IMagicPowered)
                {
                    if (((PoweredRenewableArtifact)artifact).Charge == 0)
                    {
                        DrawSpecificSituation(POWERRANOUT + '\n' + CHOOSEACTION, StandartList);
                        chooseParams = FightStatus.ChooseAction;
                    }
                    else
                    {
                        DrawSpecificSituation(CHOOSEPOWER, new List<string>() { ENTER });
                        ui.DisplayTextBox();

                        chooseParams = FightStatus.ChoosePower;
                        RememberToComeBack();
                    }                  
                }
                else
                {
                    if (enemiesPlusHero.Count > 1)
                    {
                        DrawSpecificSituation(CHOOSETARGET, EnemyNamesToList());
                                     
                        chooseParams = FightStatus.ChooseTarget;
                        RememberToComeBack();
                    }
                    else if (enemiesPlusHero.Count == 1)
                    {
                        parent.hero.Experience += plan.EXP;
                        parent.EndFight(FightResult.WON);
                    }
                }
            }
            else if (chooseParams == FightStatus.ChooseTarget)//CHOOSETARGET
            {
                target = enemiesPlusHero[index];

                if (whatNow == FightAction.HIT)
                {
                    parent.hero.Hit(target);

                    ActionsInArtifactAndSpellAndHit();
                }
                else if (whatNow == FightAction.SPELL)
                {
                    if (spell is IMagicPowered)
                    {
                        try
                        {
                            ((Magician)parent.hero).UseSpell(spell, target, power);

                            ActionsInArtifactAndSpellAndHit();
                        }
                        catch (NotEnoughManaException)
                        {
                            
                            PROBLEM = NOTENOUGHMANA;
                            ShowProblemInSpell();
                            
                        }
                        catch (Exception)
                        {
                            PROBLEM = YOUCANNOTEMOVEORTALK;
                            ShowProblemInSpell();
                        }
                    }
                    else
                    {
                        try
                        {
                            ((Magician)parent.hero).UseSpell(spell, target);

                            ActionsInArtifactAndSpellAndHit();
                        }
                        catch (NotEnoughManaException)
                        {                        
                            PROBLEM = NOTENOUGHMANA;
                            ShowProblemInSpell();                 
                        }
                        catch (Exception)
                        {                          
                            PROBLEM = YOUCANNOTEMOVEORTALK;
                            ShowProblemInSpell();
                        }
                    }
                }
                else if (whatNow == FightAction.ARTIFACT)
                {
                    if (artifact is IMagicPowered)
                    {
                        parent.hero.UseArtifact((PoweredRenewableArtifact)artifact, target, power);

                        ActionsInArtifactAndSpellAndHit();
                    }
                    else
                    {
                        parent.hero.UseArtifact(artifact, target);

                        ActionsInArtifactAndSpellAndHit();
                    }
                }                         
            }
            else if (chooseParams == FightStatus.ChooseWords)//CHOOSEWORDS
            {
                index += 1;
                if (index == variants.Count)
                {
                    DrawSpecificSituation(YOUDECIDEDTOINTERDIAL + '\n' + CHOOSEACTION, StandartList);                   
                    chooseParams = FightStatus.ChooseAction;
                }
                else if (variants.Count == 2)
                {
                    CONVERSATION += (words[0] + '\n' + answers[0][0] + '\n');
                    ui.InfoAboutCurrentConditions(CONVERSATION);

                    words.RemoveAt(0);
                    answers.RemoveAt(0);

                    if (words.Count == 0)
                    {
                        chooseParams = FightStatus.ChooseAction;
                        DrawSpecificSituation(CONVERSATION + '\n' + NOWORDS + '\n' + CHOOSEACTION, StandartList);
                    }
                    else
                    {
                        chooseParams = FightStatus.ChooseWords;

                        variants = new List<string>(answers[0]);
                        variants.Add(EXIT);

                        DrawSpecificSituation(CONVERSATION + words[0], variants);
                    }
                }
                else if (index == 1)
                {
                    MessageBox.Show(YOUAREKIND); 
                    parent.EndFight(FightResult.NEGOTIATED);
                }
                else if (index == 2)
                {
                    chooseParams = FightStatus.ChooseAction;
                    DrawSpecificSituation(CHOOSEACTION, StandartList);
                    words.RemoveAt(0);
                    InitNewRecorder();
                }
            }
            else if (chooseParams == FightStatus.ChoosePower)//CHOOSEPOWER
            {             
                try
                {
                    power = Convert.ToUInt32(ui.GetUserInputText());

                    if (power == 0)
                    {
                        throw new Exception("power cannot be 0");
                    }

                    ui.HideTextBox();

                    if (enemiesPlusHero.Count > 1)
                    {
                        DrawSpecificSituation(CHOOSETARGET, EnemyNamesToList());

                        chooseParams = FightStatus.ChooseTarget;
                        RememberToComeBack();
                    }
                    else if (enemiesPlusHero.Count == 1)
                    {
                        parent.hero.Experience += plan.EXP;
                        parent.EndFight(FightResult.WON);
                    }
                }
                catch
                {
                    ui.InfoAboutCurrentConditions(SOMETHINGWENTWRONG);
                }      
            }
        }

        private bool HeroAliveCheck()
        {
            if (parent.hero.StateHealth == StateHealth.DEAD)
            {
                enemiesPlusHero.Remove(parent.hero);
                parent.EndFight(FightResult.DIED);
                return false;
            }

            return true;
        }

        private bool EnemyAliveCheck()
        {
            if (target.StateHealth == StateHealth.DEAD)
            {
                if (target == parent.hero)
                {
                    HeroAliveCheck();
                }
                else
                {
                    StepHappened -= target.EventHandler; //unsubscribe
                    enemiesPlusHero.Remove(target);
                    ui.GetInfoEnemies(enemiesPlusHero);
                    //tell UI about murder?
                    if (enemiesPlusHero.Count == 1)
                    {
                        parent.hero.Experience += plan.EXP;
                        parent.EndFight(FightResult.WON);
                        return false;
                    }
                }
            }

            return true;
        }

        private void ActionsInArtifactAndSpellAndHit()
        {
            InitNewRecorder();

            InfoAboutPeople();

            if (!HeroAliveCheck() || !EnemyAliveCheck())
            {
                return;
            }
            
            YourEnemyReaction();
            
            StepHappened();
            if (!HeroAliveCheck() || !EnemyAliveCheck())
            {
                return;
            }
            
            InfoAboutPeople();

            recorder = new Stack<FightStatus>();
            DrawSpecificSituation(ABOUTENEMYPUNCHES + CHOOSEACTION, StandartList);

            chooseParams = FightStatus.ChooseAction;
            RememberToComeBack();
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

        private void DrawSituation(FightStatus status)
        {
            switch (status)
            {
                case FightStatus.ChooseAction:
                    {
                        DrawSpecificSituation(CHOOSEACTION, StandartList);
                    }
                    break;
                case FightStatus.ChooseTarget:
                    {
                        DrawSpecificSituation(CHOOSETARGET, EnemyNamesToList());
                    }
                    break;
                case FightStatus.ChoosePower:
                    {
                        DrawSpecificSituation(CHOOSEPOWER, new List<string>() { ENTER });                      
                        ui.DisplayTextBox();                        
                    }
                    break;
                case FightStatus.ChooseSpell:
                    {
                        DrawSpecificSituation(CHOOSESPELL, SpellNamesToList());                       
                    }
                    break;
                case FightStatus.ChooseArtifact:
                    {
                        DrawSpecificSituation(CHOOSEARTIFACT, ArtifactNamesToList());
                    }
                    break;                
                default:
                    break;
            }
        }

        private void ShowProblemInSpell()
        {
            if (spells.Count > 1)
            {
                DrawSpecificSituation(PROBLEM + '\n' + CHOOSESPELL, SpellNamesToList());
                chooseParams = FightStatus.ChooseSpell;
                InitNewRecorder();
                RememberToComeBack();
            }
            else
            {
                DrawSpecificSituation(PROBLEM + '\n' + CHOOSEACTION, StandartList);

                InitNewRecorder();
                chooseParams = FightStatus.ChooseAction;
            }
        }

        private void DrawSpecificSituation(string text, List<string> buttons)
        {
            ui.InfoAboutCurrentConditions(text);
            ui.GetInfo(buttons, buttons.Count);
        }
        
        private void InfoAboutPeople()
        {
            ui.GetInfoCharacter(parent.hero);
            ui.GetInfoEnemies(enemiesPlusHero);
        }

        private void YourEnemyReaction()
        {
            Random rnd = new Random();
            Character whoIsOnDuty = enemiesPlusHero[rnd.Next(0, enemiesPlusHero.Count - 1)];// -1 not to kill yourself
            Artifact art;
            if (!whoIsOnDuty.CanMoveNow )
            {
                ABOUTENEMYPUNCHES = ENEMYISPARALIZED;
                ui.InfoAboutCurrentConditions(ABOUTENEMYPUNCHES +'\n');
                return;
            }
            
            if ((rnd.Next(0, 2) == 0) && (whoIsOnDuty.Inventory.Count != 0))
            {
                art = whoIsOnDuty.Inventory[rnd.Next(0, whoIsOnDuty.Inventory.Count)];

                whoIsOnDuty.UseArtifact(art, parent.hero);

                ABOUTENEMYPUNCHES = THEYUSEDARTIFACT + " " + art.NAME + '\n' + WHOHITED + " " + whoIsOnDuty.Name + '\n';
            }
            else
            {
                whoIsOnDuty.Hit(parent.hero);
                if (!parent.hero.StatesDynamic.ContainsKey(State.ARMOR))
                {
                    ABOUTENEMYPUNCHES = THEYMANAGEDTOHIT + '\n' + YOULOSTHEALTHPOINTS + " " + whoIsOnDuty.HitPower + '\n' +  WHOHITED + " " + whoIsOnDuty.Name + '\n';
                }
                else
                {
                    ABOUTENEMYPUNCHES = THEYWANTEDHITBUTARMOR + '\n'+ WHOUNSUCCEDHIT + " " + whoIsOnDuty.Name + '\n';
                }
            }
            ui.InfoAboutCurrentConditions(ABOUTENEMYPUNCHES);
            InfoAboutPeople();
        }
    }
}