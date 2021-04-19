using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game
{
    class Revive : Spell
    {
        private const double MANA_COST = 150.0;

        public Revive() : base(MANA_COST, true, true) {  }

        public override void MagicEffect(Character user, Character target)
        {
            if (base.CheckCastRequirements(user))
            {
                if (target.StateHealth == StateHealth.DEAD)
                {
                    target.Revive();
                }
            }
        }
    }
}
