using game;

namespace Artifacts
{
    public sealed class LivingWater : Water
    {
        public LivingWater(BottleSize size) : base(size)
        {
            NAME = "Живая вода " + Size.ToString();
        }
        
        public LivingWater(int size) : this( (BottleSize) size)
        {
        }
        
        protected sealed override void DrinkAction(Character target)
        {
            target.Health += (int)Size;
        }
    }
}
