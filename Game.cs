using KashTaskWPF.Artifacts;
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
            this.hero = new Magician("Me", Race.HUMAN, Sex.FEMALE, 17, 45, 0, 1000);
            //hero can learn spells to be born with
            CreateFightPlanList();
        }

        private void CreateFightPlanList()
        {
            //create list of fightplans
            Character hydra = new Character("Гидра", Race.ORC, Sex.FEMALE, 48, 250, 300);

            Character goblin = new Character("Особенно Наглый Гоблин", Race.GOBLIN, Sex.MALE, 200, 200, 150);
                        
            List<string> words1 = new List<string>() { "ШШСС", "ССШИПИМ", "МЫ НЕ МОЖЕМ", "ШШШС. ЭТО НЕ ПРОКЛЯТИЕ. ЭТО МОЯ ГОЛОВА ТАНАКЕД ЗАБЫЛА ССВОИ ССЛОВА И МЫ НЕПРАВИЛЬНО ПРОИЗЗНЕССЛИ СЗАКЛИНАНИЕ.", "МНЕ НУЖНЫ ЛЯГУССШАЧЬИ ЛАПКИ."  };
            List<string> a1 = new List<string>() { "Чего шипишь, гидра?" };
            List<string> a2 = new List<string>() { "Сними проклятие, гидра, а, гидра?" };
            List<string> a3 = new List<string>() { "Всмысле не можешь?" };
            List<string> a4 = new List<string>() { "И что делать, #$#@$?" };
            List<string> a5 = new List<string>() { "Пойдем, помогу найти!", "Нет, я тебя убью!" };

            List<List<string>> answers1 = new List<List<string>>() { a1, a2, a3, a4, a5};
            List<string> words2 = new List<string>() { "Пацаны, смарите, йюху-ху, #@*%!", "#@*%%и #@*##a!"};
            List<List<string>> answers2 = new List<List<string>>() { new List<string>() { "Ну ты и дикий!" }, new List<string>() { "..." } };
            FightPlan f1 = new FightPlan(new List<Character>() { hydra }, answers1, words1, 100);
            FightPlan f2 = new FightPlan(new List<Character>() { goblin }, answers2, words2, 100);
            fightPlans.Add(f2);
            fightPlans.Add(f1);
        }        
    }
}