using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game
{
    class Unfreeze : RemoveStateSpell
    {
        public const short SpellID = 6;
        private const double MANA_COST = 85.0;
        public Unfreeze() : base(MANA_COST, true, true, State.PARALIZED)
        {
        }

    }
}
