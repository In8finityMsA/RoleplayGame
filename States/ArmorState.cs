using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using game; 

namespace KashTaskWPF.States
{
    class ArmorState : AbstractState
    {
        public ArmorState(Character carrier, int counter) : base(carrier, State.ARMOR, counter) { }
       
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
