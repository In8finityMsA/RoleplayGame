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
        private List<string> StandartList = new List<string>() { "Удар", "Заклинание", "Артефакт", "Бежать", "Поговорить" };
        private List<Character> enemies = new List<Character>();
        List<KeyValuePair<Type, Spell>> spells;
        List<Artifact> artifacts;

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

            //JsonInit();
            JoinLists();
            ui.GetInfo(StandartList, 5);
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
            for (int i = 0; i <= artifacts.Count; i++)
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
            if (chooseParams == FightStatus.ChooseAction)
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

                                //List<string> list = EnemyNamesToList();
                                //int variants = enemies.Count;

                                ui.GetInfo(EnemyNamesToList(), enemies.Count);

                                
                            }                           
                            else if (enemies.Count == 1)
                            {
                                parent.game.hero.Hit(enemies[0]);
                                ui.GetInfoEnemies(enemies);
                                if (enemies[0].StateHealth == StateHealth.DEAD)
                                {
                                    enemies.RemoveAt(0);
                                    parent.EndFight(FightResult.WON);
                                    return;
                                }
                                else
                                {
                                    YourEnemyReaction();
                                    if (parent.game.hero.StateHealth == StateHealth.DEAD)
                                    {
                                        //You are died                           
                                        parent.EndFight(FightResult.DIED);
                                        return;
                                    }
                                }
                                prevStatus = chooseParams;
                                chooseParams = FightStatus.ChooseAction;
                                ui.GetInfo(StandartList, 5);
                                                                
                            }                         
                        }                       
                        break;
                    case 2: //SPELL
                        {                  
                            if (spells.Count >= 1)
                            {
                                prevStatus = chooseParams;
                                chooseParams = FightStatus.ChooseSpell;
                                ui.GetInfo(SpellNamesToList(), spells.Count);                             
                            }
                        }
                        break;
                    case 3:  //ARTIFACT
                        {                            
                            if (artifacts.Count >= 1)
                            {
                                prevStatus = chooseParams;
                                chooseParams = FightStatus.ChooseArtifact;
                                ui.GetInfo(ArtifactNamesToList(), artifacts.Count);
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
                spell = spells[index].Value;
                if (spell is IMagicPowered)
                {
                    prevStatus = chooseParams;
                    chooseParams = FightStatus.ChoosePower;
                }
                else
                {
                    if (enemies.Count != 1)
                    {
                        prevStatus = chooseParams;
                        chooseParams = FightStatus.ChooseTarget;
                        ui.GetInfo(EnemyNamesToList(), enemies.Count);
                    }
                    else if (enemies.Count == 1)
                    {
                        target = enemies[0];
                        ((Magician)parent.game.hero).UseSpell(spell, target);
                        ui.GetInfoEnemies(enemies);
                        if (enemies[0].StateHealth == StateHealth.DEAD)
                        {
                            enemies.RemoveAt(0);
                            parent.EndFight(FightResult.WON);
                            return;
                        }
                        else
                        {
                            YourEnemyReaction();
                            if (parent.game.hero.StateHealth == StateHealth.DEAD)
                            {
                                //You are died                           
                                parent.EndFight(FightResult.DIED);
                                return;
                            }
                        }
                        prevStatus = chooseParams;
                        chooseParams = FightStatus.ChooseAction;
                        ui.GetInfo(StandartList, 5);
                        
                    }
                }
            }
            else if (chooseParams == FightStatus.ChooseArtifact)
            {
                artifact = artifacts[index];

                if (artifact is IMagicPowered)
                {
                    prevStatus = chooseParams;
                    chooseParams = FightStatus.ChoosePower;
                }
                else
                {
                    if (enemies.Count != 1)
                    {
                        prevStatus = chooseParams;
                        chooseParams = FightStatus.ChooseTarget;
                        ui.GetInfo(EnemyNamesToList(), enemies.Count);
                    }
                    else if (enemies.Count == 1)
                    {
                        target = enemies[0];
                        ((Magician)parent.game.hero).UseArtifact(artifact, target);
                        ui.GetInfoEnemies(enemies);
                        if (enemies[0].StateHealth == StateHealth.DEAD)
                        {
                            enemies.RemoveAt(0);
                            //tell about murder?
                            parent.EndFight(FightResult.WON);
                            return;
                        }
                        else
                        {
                            YourEnemyReaction();
                            if (parent.game.hero.StateHealth == StateHealth.DEAD)
                            {
                                //You are died                           
                                parent.EndFight(FightResult.DIED);
                                return;
                            }
                        }
                        prevStatus = chooseParams;
                        chooseParams = FightStatus.ChooseAction;
                        ui.GetInfo(StandartList, 5);                      
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
                            //You are died                           
                            parent.EndFight(FightResult.DIED);
                            return;
                        }
                    }
                    prevStatus = chooseParams;
                    chooseParams = FightStatus.ChooseAction;
                    ui.GetInfo(StandartList, 5);
                }
                else if (whatNow == FightAction.SPELL)
                {
                    if (spell is IMagicPowered)
                    {
                        ((Magician)parent.game.hero).UseSpell(spell, target, power);
                        ui.GetInfoEnemies(enemies);
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
                        else
                        {
                            YourEnemyReaction();
                            if (parent.game.hero.StateHealth == StateHealth.DEAD)
                            {
                                //You are died                           
                                parent.EndFight(FightResult.DIED);
                                return;
                            }
                        }
                        prevStatus = chooseParams;
                        chooseParams = FightStatus.ChooseAction;
                        ui.GetInfo(StandartList, 5);
                    }
                    else
                    {
                        ((Magician)parent.game.hero).UseSpell(spell, target);
                        ui.GetInfoEnemies(enemies);
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
                        else
                        {
                            YourEnemyReaction();
                            if (parent.game.hero.StateHealth == StateHealth.DEAD)
                            {
                                //You are died                           
                                parent.EndFight(FightResult.DIED);
                                return;
                            }
                        }
                        prevStatus = chooseParams;
                        chooseParams = FightStatus.ChooseAction;
                        ui.GetInfo(StandartList, 5);
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
                                //You are died                           
                                parent.EndFight(FightResult.DIED);
                                return;
                            }
                        }
                        prevStatus = chooseParams;
                        chooseParams = FightStatus.ChooseAction;
                        ui.GetInfo(StandartList, 5);
                    }
                    else
                    {
                        parent.game.hero.UseArtifact(artifact, target);
                        ui.GetInfoEnemies(enemies);
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
                        else
                        {
                            YourEnemyReaction();
                            if (parent.game.hero.StateHealth == StateHealth.DEAD)
                            {
                                //You are died                           
                                parent.EndFight(FightResult.DIED);
                                return;
                            }
                        }
                        prevStatus = chooseParams;
                        chooseParams = FightStatus.ChooseAction;
                        ui.GetInfo(StandartList, 5);
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
                    prevStatus = chooseParams;
                    chooseParams = FightStatus.ChooseTarget;
                    ui.GetInfo(EnemyNamesToList(), enemies.Count);
                }
                else if (enemies.Count == 1)
                {
                    if (whatNow == FightAction.SPELL)
                    {                      
                        ((Magician)parent.game.hero).UseSpell(spell, enemies[0], power);
                        ui.GetInfoEnemies(enemies);
                        if (target.StateHealth == StateHealth.DEAD)
                        {
                            enemies.RemoveAt(0);
                            //tell UI about murder?                            
                            parent.EndFight(FightResult.WON);
                            return;
                        }
                        else
                        {
                            YourEnemyReaction();
                            if (parent.game.hero.StateHealth == StateHealth.DEAD)
                            {
                                //You are died                           
                                parent.EndFight(FightResult.DIED);
                                return;
                            }
                        }
                        prevStatus = chooseParams;
                        chooseParams = FightStatus.ChooseAction;
                        ui.GetInfo(StandartList, 5);
                    }
                    else if (whatNow == FightAction.ARTIFACT)
                    {
                        parent.game.hero.UseArtifact((PoweredRenewableArtifact)artifact, enemies[0], power);
                        ui.GetInfoEnemies(enemies);
                        //enemy actions? Are you alive?
                        if (target.StateHealth == StateHealth.DEAD)
                        {
                            enemies.RemoveAt(0);
                            //tell UI about murder?                            
                            parent.EndFight(FightResult.WON);
                            return;                            
                        }
                        else
                        {
                            YourEnemyReaction();
                            if (parent.game.hero.StateHealth == StateHealth.DEAD)
                            {
                                //You are died                           
                                parent.EndFight(FightResult.DIED);
                                return;
                            }
                        }
                        prevStatus = chooseParams;
                        chooseParams = FightStatus.ChooseAction;
                        ui.GetInfo(StandartList, 5);
                    }
                }           
            }
        }

        public void YourEnemyReaction()
        {
            Random rnd = new Random();
            Character whoIsOnDuty = enemies[rnd.Next(0, enemies.Count - 1)];
            Artifact art;

            if (whoIsOnDuty.Inventory.Count != 0)
            {
                if (rnd.Next(0, 1) == 0)
                {
                    art = whoIsOnDuty.Inventory[rnd.Next(0, whoIsOnDuty.Inventory.Count - 1)];
                    whoIsOnDuty.UseArtifact(art, parent.game.hero);
                }
                else
                {
                    whoIsOnDuty.Hit(parent.game.hero);
                }
            }
            else
            {
                whoIsOnDuty.Hit(parent.game.hero);
            }
            ui.GetInfoCharacter(parent.game.hero);
        }
    }
}
