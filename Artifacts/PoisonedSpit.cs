using game;
using KashTaskWPF.States;

namespace KashTaskWPF.Artifacts
{
    public sealed class PoisonedSpit : PoweredRenewableArtifact
    {
        private const int period = 10;
        public readonly int steps;
        public PoisonedSpit(int charge, int steps = period) : base(charge)
        {
            NAME = $"Ядовитая слюна ({Charge})";
            this.steps = steps;
        }      
        protected sealed override void ActionsWithState(Character target)
        {
            PoisonedState stateToAdd = new PoisonedState(target, steps);
            target.AddStateD(stateToAdd);
        }
        public sealed override double ConsumptionPerPower => 1;
    }
}
