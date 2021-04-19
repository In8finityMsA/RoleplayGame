using game;
using KashTask;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using KASHGAMEWPF;
using static System.Net.Mime.MediaTypeNames;
using KashTaskWPF.Adapters;
using KashTaskWPF;
using Artifacts;

namespace KASHGAMEWPF
{
    public enum FightStatus
    {
        ChooseAction, ChooseTarget, ChoosePower, ChooseSpell, ChooseArtifact, ChooseWords
    }
    public enum FightAction
    {
        HIT = 1, SPELL = 2, ARTIFACT = 3, TALK = 4, RUN = 5
    }
    class Fighter: IAdapter
    {
        private readonly string fileName;
        private FightPlan plan;

        private List<Character> enemies;
        List<KeyValuePair<Type, Spell>> listOfSpells;

        private Stager parent;

        private FightAction whatNow;
        private FightStatus chooseParams = FightStatus.ChooseAction;
        private Character target;
        private Spell spell;
        private double power;
        private Artifact artifact;

        public Fighter(string fileName, Stager parent)
        {
            this.fileName = fileName;
            this.parent = parent;
            JsonInit();
            JoinLists();
        }

        private void JoinLists()
        {
            List<Character> magaschar = new List<Character>();
            for (int i = 0; i < plan.enemyM.Count; i++)
            {
                magaschar[i] = (Character)plan.enemyM[i];
            }
            enemies.AddRange(magaschar);
            enemies.AddRange(plan.enemyNonM);
        }

        public void JsonInit()
        {
            var reader = new StreamReader(fileName);
            var jsonString = reader.ReadToEnd();
            reader.Close();
            plan = JsonSerializer.Deserialize<FightPlan>(jsonString);
        }

        public string EnemyNamesToString()
        {
            string result = "";//form string to UI
            for (int i = 0; i < enemies.Count; i++)
            {
                result += i + ". " + enemies[i].Name + '\n';
            }
            return result;
        }

        public string SpellNamesToString()
        {
            string result = "";
            listOfSpells = ((Magician)parent.game.hero).Spells.ToList<KeyValuePair<Type, Spell>>();
            for (int i = 0; i < listOfSpells.Count; i++)
            {
                result += i + ". " + listOfSpells[i].Key.ToString() + "\n";
            }
            return result;
        }

        public void GetInput(int index)
        {
            if (chooseParams == FightStatus.ChooseAction)
            {
                whatNow = (FightAction)index;
                switch (index)
                {
                    case 1: //HIT
                        {
                            if ( enemies.Count > 1)
                            {
                                chooseParams = FightStatus.ChooseTarget;
                                KashTaskWPF.MainWindow.mainwindow.GetInfo(EnemyNamesToString(), enemies.Count);
                            }                           
                            else if (enemies.Count == 1)
                            {
                                parent.game.hero.Hit(enemies[0]);
                                //your enemy action?
                                if (enemies[0].StateHealth == StateHealth.DEAD)
                                {
                                    enemies.RemoveAt(0);
                                    parent.EndFight(true);
                                    return;
                                }
                                else
                                {
                                    chooseParams = FightStatus.ChooseAction;
                                }                                
                            }                         
                        }                       
                        break;
                    case 2: //SPELL
                        {                  
                            string text = SpellNamesToString(); 
                            if (listOfSpells.Count > 1)
                            {
                                chooseParams = FightStatus.ChooseSpell;//wait for choosing spell
                                // send it for UI and list
                                KashTaskWPF.MainWindow.mainwindow.GetInfo(text, listOfSpells.Count);//send info to UI                              
                            }
                            //else if (listOfSpells.Count == 0)
                            //{
                            //    chooseParams = FightStatus.ChooseAction;
                            //    return;
                            //}
                            //else if (listOfSpells.Count == 1)
                            //{
                            //    spell = listOfSpells[0].Value;
                            //    if (spell is IMagicPowered)
                            //    {
                            //        chooseParams = FightStatus.ChoosePower;
                            //    }
                            //    else
                            //    {
                            //        if (enemies.Count != 1)
                            //        {
                            //            chooseParams = FightStatus.ChooseTarget;
                            //            KashTaskWPF.MainWindow.mainwindow.GetInfo(EnemyNamesToString(), enemies.Count);
                            //        }
                            //        else if (enemies.Count == 1)
                            //        {
                            //            target = enemies[0];
                            //            ((Magician)parent.game.hero).UseSpell(spell, target);
                            //            //enemy action? Are you alive?
                            //            if (enemies[0].StateHealth == StateHealth.DEAD)
                            //            {
                            //                enemies.RemoveAt(0);
                            //                parent.EndFight(true);
                            //                return;
                            //            }
                            //            else
                            //            {
                            //                chooseParams = FightStatus.ChooseAction;
                            //            }
                            //        }
                            //    }
                                
                                

                            //}



                        }
                        break;
                    case 3:  //ARTIFACT
                        {
                            
                        }
                        break;
                    case 4:  //TALK
                        {
                            
                        }
                        break;
                    case 5: //RUN
                        {
                            
                        }
                        break;
                    default:
                        
                        break;
                }                
            }
            else if (chooseParams == FightStatus.ChooseSpell)
            {
                spell = listOfSpells[index].Value;
                if (spell is IMagicPowered)
                {
                    chooseParams = FightStatus.ChoosePower;
                }
                else
                {
                    if (enemies.Count != 1)
                    {
                        chooseParams = FightStatus.ChooseTarget;
                        KashTaskWPF.MainWindow.mainwindow.GetInfo(EnemyNamesToString(), enemies.Count);
                    }
                    else if (enemies.Count == 1)
                    {
                        target = enemies[0];
                        ((Magician)parent.game.hero).UseSpell(spell, target);
                        //enemy action? Are you alive?
                        if (enemies[0].StateHealth == StateHealth.DEAD)
                        {
                            enemies.RemoveAt(0);
                            parent.EndFight(true);
                            return;
                        }
                        else
                        {
                            chooseParams = FightStatus.ChooseAction;
                        }
                    }
                }
            }
            else if (chooseParams == FightStatus.ChooseArtifact)
            {

            }
            else if (chooseParams == FightStatus.ChooseTarget)
            {
                target = enemies[index];

                if (whatNow == FightAction.HIT)
                {
                    parent.game.hero.Hit(target);
                    //your enemy action? are you alive?
                    if (target.StateHealth == StateHealth.DEAD)
                    {
                        enemies.Remove(target);
                        //tell UI about murder?
                        if (enemies.Count == 0)
                        {
                            parent.EndFight(true);
                            return;
                        }
                    }
                    chooseParams = FightStatus.ChooseAction;
                }
                else if (whatNow == FightAction.SPELL)
                {
                    if (spell is IMagicPowered)
                    {
                        ((Magician)parent.game.hero).UseSpell(spell, target, power);
                        //enemy actions? are you alive?
                        if (target.StateHealth == StateHealth.DEAD)
                        {
                            enemies.Remove(target);
                            //tell UI about murder?
                            if (enemies.Count == 0)
                            {
                                parent.EndFight(true);
                                return;
                            }
                        }
                        chooseParams = FightStatus.ChooseAction;
                    }
                    else
                    {
                        ((Magician)parent.game.hero).UseSpell(spell, target);
                        //enemy actions? are you alive?
                        if (target.StateHealth == StateHealth.DEAD)
                        {
                            enemies.Remove(target);
                            //tell UI about murder?
                            if (enemies.Count == 0)
                            {
                                parent.EndFight(true);
                                return;
                            }
                        }
                        chooseParams = FightStatus.ChooseAction;
                    }

                }
                else if (whatNow == FightAction.ARTIFACT)
                {

                }
                else if (whatNow == FightAction.TALK)
                {

                }
                
            }
            else if (chooseParams == FightStatus.ChooseWords)
            {

            }
            else if (chooseParams == FightStatus.ChoosePower)
            {
                power = index;
                if (enemies.Count > 1)
                {
                    chooseParams = FightStatus.ChooseTarget;
                    KashTaskWPF.MainWindow.mainwindow.GetInfo(EnemyNamesToString(), enemies.Count);
                }
                else if (enemies.Count == 1)
                {
                    if (whatNow == FightAction.SPELL)
                    {
                        ((Magician)parent.game.hero).UseSpell(spell, enemies[0], power);
                        //enemy actions? Are you alive?
                        if (target.StateHealth == StateHealth.DEAD)
                        {
                            enemies.RemoveAt(0);
                            //tell UI about murder?                            
                            parent.EndFight(true);
                            return;
                        }
                        chooseParams = FightStatus.ChooseAction;
                    }
                    else if (whatNow == FightAction.ARTIFACT)
                    {
                        parent.game.hero.UseArtifact((PoweredRenewableArtifact)artifact, enemies[0], power);
                        //enemy actions? Are you alive?
                        if (target.StateHealth == StateHealth.DEAD)
                        {
                            enemies.RemoveAt(0);
                            //tell UI about murder?                            
                            parent.EndFight(true);
                            return;                            
                        }
                        chooseParams = FightStatus.ChooseAction;
                    }
                }           
            }

        }


    }
}
