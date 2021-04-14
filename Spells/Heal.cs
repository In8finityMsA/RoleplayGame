using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game
{
    class Heal : RemoveStateSpell
    {
        public static readonly short SpellID = SpellIDManager.GetNextID();
        public override short GetClassID()
        {
            return SpellID;
        }

        private const double MANA_COST = 30.0;
        public Heal() : base(MANA_COST, false, true, State.SICK)
        {
        }
    }
}
