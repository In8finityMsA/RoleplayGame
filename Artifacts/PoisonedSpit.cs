using game;
using KashTaskWPF.States;

namespace KashTaskWPF.Artifacts
{
    public sealed class PoisonedSpit : PoweredRenewableArtifact
    {
        private const int period = 2;
        public readonly int steps;
        public PoisonedSpit(int charge, int steps = period) : base(charge, "Ядовитая слюна")
        {
            this.steps = steps;
        }
        public PoisonedSpit(int charge): base(charge, "Ядовитая слюна")
        {
            this.steps = period;
        }
        protected sealed override void ActionsWithState(Character target)
        {
            PoisonedState stateToAdd = new PoisonedState(target, steps);
            target.AddStateD(stateToAdd);
        }
        public sealed override double ConsumptionPerPower => 1;
    }
}
