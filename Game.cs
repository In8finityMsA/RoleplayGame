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
            Character goblin = new Character("Особенно Наглый Гоблин", Race.GOBLIN, Sex.MALE, 200, 200, 150);
            Character hydra = new Character("Гидра", Race.ORC, Sex.FEMALE, 48, 250, 300);
            
            List<string> words1 = new List<string>() { "ШШСС", "ССШИПИМ", "МЫ НЕ МОЖЕМ", "ШШШС. ЭТО НЕ ПРОКЛЯТИЕ. ЭТО МОЯ ГОЛОВА ТАНАКЕД ЗАБЫЛА ССВОИ ССЛОВА И МЫ НЕПРАВИЛЬНО ПРОИЗЗНЕССЛИ СЗАКЛИНАНИЕ.", "МНЕ НУЖНЫ ЛЯГУССШАЧЬИ ЛАПКИ.", "ДА"  };
            List<string> answers1 = new List<string>() { "Чего шипишь, гидра?", "Сними проклятие, гидра, а, гидра?", "Всмысле не можешь?", "И что делать, #$#@$?", "Декокт из лягушачьих лапок подойдет?", "..."};
            
            List<string> words2 = new List<string>() { "Пацаны, смарите, йюху-ху, #@*%!", "#@*%%и #@*##a!"};
            List<string> answers2 = new List<string>() { "Ну ты и дикий!", "..." };
            FightPlan f1 = new FightPlan(new List<Character>() { hydra }, answers1, words1);
            FightPlan f2 = new FightPlan(new List<Character>() { goblin }, answers2, words2);
            fightPlans.Add(f1);
            fightPlans.Add(f2);
        }        
    }
}