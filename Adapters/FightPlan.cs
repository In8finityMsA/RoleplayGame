using game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KashTaskWPF.Adapters
{
    class FightPlan
    {
        public FightPlan() { }

        public List<Magician> enemyM { get; set; }
        public List<Character> enemyNonM { get; set; }
        public List<Dictionary<string, List<string>>> dialog;

    }
}
