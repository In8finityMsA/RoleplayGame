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

        protected override void ActionOnAdd()
        {
        }
        
        protected override void ActionOnRemove()
        {
        }

        protected override void ActionOnTick()
        {
        }
    }
}
