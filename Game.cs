using KashTaskWPF.Artifacts;
using game;
using KashTaskWPF.Adapters;
using System.Collections.Generic;
using KashTaskWPF.Spells;

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
            Magician Anna = new Magician("Аннушка", Race.ELF, Sex.FEMALE, 12, 1000, 20, 23);
            Character Mark = new Character("Марк", Race.ELF, Sex.MALE, 23, 2000, 20);
            //hero.PickUpArtifact(new FrogLegDecoct());
            //hero.PickUpArtifact(new VasiliskEye());
            //hero.PickUpArtifact(new LivingWater(BottleSize.L));
            ((Magician)hero).LearnSpell(typeof(AddHealth));
            //((Magician)hero).LearnSpell(typeof(Heal));
            ((Magician)hero).LearnSpell(typeof(Armor));
            hero.CanMoveNow = false;
            hero.CanSpeakNow = false;
            //Anna.PickUpArtifact(new PoisonedSpit(300));
            Anna.PickUpArtifact(new VasiliskEye());
            //Anna.PickUpArtifact(new VasiliskEye());
            //Mark.PickUpArtifact(new VasiliskEye());
            //Mark.PickUpArtifact(new VasiliskEye());
            FightPlan f1 = new FightPlan(new List<Magician>() { Anna },
                new List<Character>() { Mark }, null, null);
            fightPlans.Add(f1);
        }        
    }
}