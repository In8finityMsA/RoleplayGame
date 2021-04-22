using System;
using game;

namespace KashTaskWPF.Artifacts
{
    public sealed class LivingWater : Water
    {
        public LivingWater(BottleSize size) : base(size)
        {
            NAME = "Живая вода " + Size.ToString();
        }
        
        public LivingWater(int size) : base(size)
        {
            NAME = "Живая вода " + Size.ToString();
        }
        
        protected sealed override void DrinkAction(Character target)
        {
            target.Health += (int)Size;
        }
    }
}
