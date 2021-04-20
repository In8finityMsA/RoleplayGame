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
using KashTaskWPF.Adapters;
using static System.Net.Mime.MediaTypeNames;

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
        private Stager parent;

        private FightAction whatNow;
        private FightStatus chooseParams = FightStatus.ChooseAction;

        public Fighter(string fileName, Stager parent)
        {
            this.fileName = fileName;
            this.parent = parent;
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
                            int enemyAmount = plan.enemyM.Count + plan.enemyNonM.Count;
                            if ( enemyAmount > 1)
                            {
                                chooseParams = FightStatus.ChooseTarget;
                            }
                            else if (enemyAmount == 0)
                            {
                                //parent.WonTheFight();
                                //KASHGAMEWPF.MainWindow.mainWindow.FightResults(true);
                                //Application.Current.MainWindow._frameCounter.FightResults(true);
                            }
                            else
                            {
                                //character.Hit();
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
