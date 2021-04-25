using game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KashTaskWPF.States
{
    public class SickState : AbstractState
    {
        public SickState(Character carrier, int counter) : base(carrier, State.SICK, counter) { }
        
        public override void Step()
        {
            if (counter > 0)
            {
                counter--;
                //TODO
            }
            else
            {
                carrier.RemoveStateD(State);
            }
        }
    }
}
