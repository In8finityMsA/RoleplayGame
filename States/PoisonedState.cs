using game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KashTaskWPF.States
{
    public class PoisonedState : AbstractState
    {
        private double periodicDamage;

        public PoisonedState(Character carrier, int counter, double periodicDamage) : base(carrier, State.POISONED,
            counter)
        {
            this.periodicDamage = periodicDamage;
        }

        protected override void ActionOnAdd()
        {
        }
        
        protected override void ActionOnRemove()
        {
        }

        protected override void ActionOnTick()
        {
            carrier.Health -= periodicDamage;
        }
    }
}
