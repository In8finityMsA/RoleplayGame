using Artifacts;

namespace game
{
    public sealed class LivingWater : Water
    {
        public LivingWater(BottleSize size) : base(size) { }

        protected sealed override void ActionsWithHealth(Character target)
        {
            target.Health += (int)Size;
            NAME = "Живая вода " + Size.ToString();
        }
    }
}
