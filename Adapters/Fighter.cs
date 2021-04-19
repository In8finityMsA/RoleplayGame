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
    class Fighter: IAdapter
    {
        private readonly string fileName;
        private FightPlan plan;
        private const string StandardString = "1. Удар" + "\n" + "2. Заклинание" + "\n" + "3. Артефакт" + "\n" + "4. Бежать" + "\n" + "5. Поговорить";
        private List<Character> enemies;
        List<KeyValuePair<Type, Spell>> listOfSpells;
        List<Artifact> artifacts;

        private Stager parent;

        private FightAction whatNow;
        private FightStatus chooseParams = FightStatus.ChooseAction;
        private Character target;
        private Spell spell;
        private double power;
        private Artifact artifact;
        private MainWindow ui = KashTaskWPF.MainWindow.mainwindow;

        public Fighter(string fileName, Stager parent)
        {
            this.fileName = fileName;
            this.parent = parent;
            JsonInit();
            JoinLists();
            ui.GetInfo(StandardString, 5);
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

        public string ArtifactNamesToString()
        {

            string result = "";
            artifacts = parent.game.hero.Inventory;
            for (int i = 0; i < artifacts.Count; i++)
            {
                result += i + ". " + artifacts[i].GetType().ToString() + "\n";
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
                                ui.GetInfo(EnemyNamesToString(), enemies.Count);
                            }                           
                            else if (enemies.Count == 1)
                            {
                                parent.game.hero.Hit(enemies[0]);
                                //your enemy action?
                                if (enemies[0].StateHealth == StateHealth.DEAD)
                                {
                                    enemies.RemoveAt(0);
                                    parent.EndFight(FightResult.WON);
                                    return;
                                }
                                else
                                {
                                    chooseParams = FightStatus.ChooseAction;
                                    ui.GetInfo(StandardString, 5);
                                }                                
                            }                         
                        }                       
                        break;
                    case 2: //SPELL
                        {                  
                            if (listOfSpells.Count >= 1)
                            {
                                chooseParams = FightStatus.ChooseSpell;
                                ui.GetInfo(SpellNamesToString(), listOfSpells.Count);                             
                            }
                        }
                        break;
                    case 3:  //ARTIFACT
                        {                            
                            if (artifacts.Count >= 1)
                            {
                                chooseParams = FightStatus.ChooseArtifact;
                                ui.GetInfo(ArtifactNamesToString(), artifacts.Count);
                            }
                        }
                        break;
                    case 4:  //TALK
                        {
                            
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
                        ui.GetInfo(EnemyNamesToString(), enemies.Count);
                    }
                    else if (enemies.Count == 1)
                    {
                        target = enemies[0];
                        ((Magician)parent.game.hero).UseSpell(spell, target);
                        //enemy action? Are you alive?
                        if (enemies[0].StateHealth == StateHealth.DEAD)
                        {
                            enemies.RemoveAt(0);
                            parent.EndFight(FightResult.WON);
                            return;
                        }
                        else
                        {
                            chooseParams = FightStatus.ChooseAction;
                            ui.GetInfo(StandardString, 5);
                        }
                    }
                }
            }
            else if (chooseParams == FightStatus.ChooseArtifact)
            {
                artifact = artifacts[index];

                if (artifact is IMagicPowered)
                {
                    chooseParams = FightStatus.ChoosePower;
                }
                else
                {
                    if (enemies.Count != 1)
                    {
                        chooseParams = FightStatus.ChooseTarget;
                        ui.GetInfo(EnemyNamesToString(), enemies.Count);
                    }
                    else if (enemies.Count == 1)
                    {
                        target = enemies[0];
                        ((Magician)parent.game.hero).UseArtifact(artifact, target);
                        //enemy action? Are you alive?
                        if (enemies[0].StateHealth == StateHealth.DEAD)
                        {
                            enemies.RemoveAt(0);
                            parent.EndFight(FightResult.WON);
                            return;
                        }
                        else
                        {
                            chooseParams = FightStatus.ChooseAction;
                            ui.GetInfo(StandardString, 5);
                        }
                    }
                }
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
                            parent.EndFight(FightResult.WON);
                            return;
                        }
                    }
                    chooseParams = FightStatus.ChooseAction;
                    ui.GetInfo(StandardString, 5);
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
                                parent.EndFight(FightResult.WON);
                                return;
                            }
                        }
                        chooseParams = FightStatus.ChooseAction;
                        ui.GetInfo(StandardString, 5);
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
                                parent.EndFight(FightResult.WON);
                                return;
                            }
                        }
                        chooseParams = FightStatus.ChooseAction;
                        ui.GetInfo(StandardString, 5);
                    }

                }
                else if (whatNow == FightAction.ARTIFACT)
                {
                    if (artifact is IMagicPowered)
                    {
                        parent.game.hero.UseArtifact((PoweredRenewableArtifact)artifact, target, power);
                        //enemy actions? are you alive?
                        if (target.StateHealth == StateHealth.DEAD)
                        {
                            enemies.Remove(target);
                            //tell UI about murder?
                            if (enemies.Count == 0)
                            {
                                parent.EndFight(FightResult.WON);
                                return;
                            }
                        }
                        chooseParams = FightStatus.ChooseAction;
                        ui.GetInfo(StandardString, 5);
                    }
                    else
                    {
                        parent.game.hero.UseArtifact(artifact, target);
                        //enemy actions? are you alive?
                        if (target.StateHealth == StateHealth.DEAD)
                        {
                            enemies.Remove(target);
                            //tell UI about murder?
                            if (enemies.Count == 0)
                            {
                                parent.EndFight(FightResult.WON);
                                return;
                            }
                        }
                        chooseParams = FightStatus.ChooseAction;
                        ui.GetInfo(StandardString, 5);
                    }
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
                    ui.GetInfo(EnemyNamesToString(), enemies.Count);
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
                            parent.EndFight(FightResult.WON);
                            return;
                        }
                        chooseParams = FightStatus.ChooseAction;
                        ui.GetInfo(StandardString, 5);
                    }
                    else if (whatNow == FightAction.ARTIFACT)
                    {
                        parent.game.hero.UseArtifact((PoweredRenewableArtifact)artifact, enemies[0], power);                        
                        //enemy actions? Are you alive?
                        if (target.StateHealth == StateHealth.DEAD)
                        {
                            enemies.RemoveAt(0);
                            //tell UI about murder?                            
                            parent.EndFight(FightResult.WON);
                            return;                            
                        }
                        chooseParams = FightStatus.ChooseAction;
                        ui.GetInfo(StandardString, 5);
                    }
                }           
            }
        }
    }
}
