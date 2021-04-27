using game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KashTaskWPF.States
{
    public class ParalizedState : AbstractState
    {
        public ParalizedState(Character carrier, int counter) : base(carrier, State.PARALIZED, counter) { }
        
        public override void Step()
        {
            if (counter > 0)
            {
                counter--;
                carrier.CanMoveNow = false;
                //TODO
            }
            else
            {
                carrier.RemoveStateD(State);
            }
        }
    }
}
