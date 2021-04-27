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
        public FightPlan(List<Character> EnemyList, List<string> yourWord, List<string> enemiesWord)
        {
            this.EnemyList = EnemyList;
            this.yourWord = yourWord;
            this.enemiesWord = enemiesWord;
        }
        
        public List<Character> EnemyList { get; set; }
        public List<string> yourWord { get; set; }
        public List<string> enemiesWord { get; set; }

    }
}
