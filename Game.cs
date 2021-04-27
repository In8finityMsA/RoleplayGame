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
        public Character Gnom = new Character("Дварфыч", Race.GNOME, Sex.MALE, 300, 50, 250);
        public List<FightPlan> fightPlans = new List<FightPlan>();
        
        public Game()
        {
            this.hero = new Magician("Me", Race.HUMAN, Sex.FEMALE, 17, 1000, 300, 1000);
            //hero can learn spells to be born with
            CreateFightPlanList();
        }

        private void CreateFightPlanList()
        {
            //create list of fightplans
            Magician Anna = new Magician("Аннушка", Race.ELF, Sex.FEMALE, 12, 20, 20, 23);
            Character Mark = new Character("Марк", Race.ELF, Sex.MALE, 23, 50, 20);
            //hero.PickUpArtifact(new FrogLegDecoct());
            //hero.PickUpArtifact(new VasiliskEye());
            //hero.PickUpArtifact(new LivingWater(BottleSize.L));
            //((Magician)hero).LearnSpell(typeof(AddHealth));
            //((Magician)hero).LearnSpell(typeof(Heal));
            hero.LearnSpell(typeof(Armor));
            //hero.CanMoveNow = false;
            //hero.CanSpeakNow = false;
            //Anna.PickUpArtifact(new PoisonedSpit(300));
            Anna.PickUpArtifact(new VasiliskEye());
            List<string> words = new List<string>() { "1", "3", "5" };
            List<string> answers = new List<string>() { "2", "4", "6" };
            //Anna.PickUpArtifact(new VasiliskEye());
            //Mark.PickUpArtifact(new VasiliskEye());
            //Mark.PickUpArtifact(new VasiliskEye());
            FightPlan f1 = new FightPlan(new List<Character>() { Anna, Mark }, answers, words);
            fightPlans.Add(f1);
        }        
    }
}