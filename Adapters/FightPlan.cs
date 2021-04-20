using game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KashTaskWPF.Adapters
{
    public class FightPlan
    {
        public FightPlan() { }
        public FightPlan(List<Magician> magicEnemyList, List<Character> usualEnemyList, List<string> yourWord, List<string> enemiesWord)
        {
            enemyM = magicEnemyList;
            enemyNonM = usualEnemyList;
            this.yourWord = yourWord;
            this.enemiesWord = enemiesWord;
        }
        public List<Magician> enemyM { get; set; }
        public List<Character> enemyNonM { get; set; }
        public List<string> yourWord { get; set; }
        public List<string> enemiesWord { get; set; }

    }
}
