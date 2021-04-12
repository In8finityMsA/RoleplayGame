using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game
{
    class Revive : Spell
    {
        public const short SpellID = 4;
        private const double MANA_COST = 150.0;

        public Revive() : base(MANA_COST, true, true)
        {
        }

        public override void MagicEffect(Magician user, Character target)
        {
            if (base.CheckCastRequirements(user))
            {
                if (target.StateHealth == StateHealth.DEAD)
                {
                    user.Mana -= MANA_COST;
                    user.Health = 1;
                }
            }
        }

        public override void MagicEffect(Magician user)
        {
            MagicEffect(user, user); //Is it even possible??
        }
    }
}
