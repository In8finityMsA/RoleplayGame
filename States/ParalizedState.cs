using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KashTaskWPF.States
{
    public class ParalizedState : AbstractState
    {
        public ParalizedState(Character carrier, int counter) : base(carrier, State.PARALIZED, counter)
        {
            ActionOnAdd();
        }

        protected override void ActionOnAdd()
        {
            carrier.CanMoveNow = false;
        }
        
        protected override void ActionOnRemove()
        {
            carrier.CanMoveNow = true;
        }

        protected override void ActionOnTick()
        {
            carrier.CanMoveNow = false;
        }
    }
}
