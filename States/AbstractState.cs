using game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KashTaskWPF.States
{
    abstract class AbstractState: IStatable
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
        public State State { get => State; }
        public Character Carrier { get => carrier; }
      
        abstract public void Step();
    }
}
