using game;

namespace Artifacts
{
    public sealed class Thunder : PoweredRenewableArtifact
    {
        public Thunder(int charge) : base(charge) { }
        protected sealed override void ActionsWithState(Character target) { }
        public sealed override double ConsumptionPerPower => 1;
    }
}
