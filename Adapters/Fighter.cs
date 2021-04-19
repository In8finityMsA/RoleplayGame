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

        private Stager parent;

        private FightAction whatNow;
        private FightStatus chooseParams = FightStatus.ChooseAction;

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

        public void GetInput(int index)
        {
            if (chooseParams == FightStatus.ChooseAction)
            {
                whatNow = (FightAction)index;
                switch (index)
                {
                    case 1:
                        {
                            if ( enemies.Count > 1)
                            {
                                chooseParams = FightStatus.ChooseTarget;

                                string str = "";//form string to UI
                                for (int i = 0; i < enemies.Count; i++)
                                {
                                    str += i + ". " + enemies[i].Name + '\n';
                                }
                                
                            }
                            else if (enemies.Count == 0)
                            {
                                parent.EndFight(true);
                            }
                            else
                            {
                                parent.game.hero.Hit(enemies[0]);
                                if (enemies[0].StateHealth == StateHealth.DEAD)
                                {
                                    enemies.RemoveAt(0);
                                    parent.EndFight(true);
                                }                                
                            }                         
                        }                       
                        break;
                    case 2:
                        {
                            //whatNow = (FightAction)index;
                        }
                        break;
                    case 3:
                        {
                            //whatNow = (FightAction)index;
                        }
                        break;
                    case 4:
                        {
                            //whatNow = (FightAction)index;
                        }
                        break;
                    case 5:
                        {
                            //whatNow = (FightAction)index;
                        }
                        break;
                    default:
                        
                        break;
                }                
            }
            else if (chooseParams == FightStatus.ChooseSpell)
            {

            }
            else if (chooseParams == FightStatus.ChooseArtifact)
            {

            }
            else if (chooseParams == FightStatus.ChooseTarget)
            {

            }
            else if (chooseParams == FightStatus.ChooseWords)
            {

            }
            else if (chooseParams == FightStatus.ChoosePower)
            {

            }

        }


    }
}
