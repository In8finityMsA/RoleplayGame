using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KashTaskWPF.States
{
    class ArmorState : AbstractState
    {
        public ArmorState(Character carrier, int counter) : base(carrier, State.ARMOR, counter) { }

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
