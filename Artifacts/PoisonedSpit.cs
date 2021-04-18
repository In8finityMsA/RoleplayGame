using game;

namespace Artifacts
{
    public sealed class PoisonedSpit : PoweredRenewableArtifact
    {
        public PoisonedSpit(int charge) : base(charge) { }      
        protected sealed override void ActionsWithState(Character target) => target.AddState(State.POISONED);
        public sealed override double ConsumptionPerPower => 1;
    }
}
