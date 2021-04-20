using Artifacts;
using game;
using KashTaskWPF.Adapters;
using System.Collections.Generic;

namespace KashTaskWPF
{
    public class Game
    {
        public Magician hero;
        public List<FightPlan> fightPlans;
        
        public Game(Magician hero)
        {
            this.hero = hero;
            //hero can learn spells to be born with
            CreateFightPlanList();
        }

        private void CreateFightPlanList()
        {
            //create list of fightplans
        }        
    }
}