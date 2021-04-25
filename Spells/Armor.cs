using game;
using KashTaskWPF.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KashTaskWPF.Spells
{
    class Armor : AddStateSpell, IMagicPowered
    {
        private const double MANA_COST = 10;// to much?

        public Armor() : base(MANA_COST, false, false) { }

        public double ConsumptionPerPower => MANA_COST; 

        public override AbstractState CreateState(Character carrier, int steps)
        {
            return new ArmorState(carrier, steps);
        }

        public void MagicEffect(Character user, Character target, double power)
        {
            if (base.CheckCastRequirements(user) && target.StateHealth != StateHealth.DEAD)
            {
                Magician magicUser = (Magician)user;
                
                if (magicUser.Mana >= (int)power * ConsumptionPerPower)
                {
                    magicUser.Mana -= (int)power * ConsumptionPerPower;
                    target.AddStateD(CreateState(target, (int)power));
                }
                else
                {
                    if ((int)(magicUser.Mana / ConsumptionPerPower) > 0)
                    {
                        target.AddStateD(CreateState(target, (int)(magicUser.Mana / ConsumptionPerPower)));
                    }                 
                }
            }
        }
    }
}
