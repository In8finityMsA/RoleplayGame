using game;

namespace KashTaskWPF.Artifacts
{
    public sealed class PoisonedSpit : PoweredRenewableArtifact
    {
        public PoisonedSpit(int charge) : base(charge)
        {
            NAME = $"Ядовитая слюна ({Charge})";
        }      
        protected sealed override void ActionsWithState(Character target) => target.AddState(State.POISONED);
        public sealed override double ConsumptionPerPower => 1;
    }
}
