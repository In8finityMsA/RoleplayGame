using game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KashTaskWPF.States
{
    class PoisonedState : AbstractState
    {
        public PoisonedState(Character carrier, int counter) : base(carrier, State.POISONED, counter) { }

        public override void Step()
        {
            if (counter != 0)
            {
                counter--;
                //TODO
            }
        }
    }
}
