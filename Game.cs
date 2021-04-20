using Artifacts;
using game;
using KashTaskWPF.Adapters;
using System.Collections.Generic;

namespace KashTaskWPF
{
    public class Game
    {
        public Magician hero;
        public List<FightPlan> fightPlans = new List<FightPlan>();
        
        public Game(Magician hero)
        {
            this.hero = hero;
            //hero can learn spells to be born with
            CreateFightPlanList();
        }

        private void CreateFightPlanList()
        {
            //create list of fightplans
            FightPlan f1 = new FightPlan(new List<Magician>() { new Magician("Anna", Race.ELF, Sex.FEMALE, 12, 16, 20, 23) },
                new List<Character>() { new Character("Mark", Race.ELF, Sex.MALE, 23, 20, 20) }, null, null);
            fightPlans.Add(f1);
        }        
    }
}