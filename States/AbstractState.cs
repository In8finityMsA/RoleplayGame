using game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KashTaskWPF.States
{
    public abstract class AbstractState
    {
        protected int counter;
        protected State state;
        protected Character carrier;
        public AbstractState(Character character, State state, int counter)
        {
            this.carrier = character;
            this.state = state;
            this.counter = counter;
        }
        public int Counter { get => counter; }
        public State State { get => state; }
        public Character Carrier { get => carrier; }

        protected void CounterTick()
        {
            counter--;
            ActionOnTick();
            if (counter <= 0) 
            {
                ActionOnRemove();
                carrier.RemoveStateD(State);
            }
        }

        protected abstract void ActionOnRemove();
        protected abstract void ActionOnTick();
        protected abstract void ActionOnAdd();

        public virtual void Step()
        {
            CounterTick();
        }
    }
}
