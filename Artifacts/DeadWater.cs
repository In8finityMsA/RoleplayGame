using KashTaskWPF.Artifacts;
using game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KashTaskWPF.Artifacts
{
    public sealed class DeadWater: Water
    {
        public DeadWater(BottleSize size) : base(size) 
        {
            NAME = "Мертвая вода " + Size.ToString();
        }
        
        public DeadWater(int size) : base(size)
        {
            NAME = "Мертвая вода " + Size.ToString();
        }

        protected sealed override void DrinkAction(Character target)
        {
            if (target is Magician)
            {
                (target as Magician).Mana += (int) Size;
            }
            else
            {
                target.Health -= (int) Size;
            }
        }
    }
}
