using game;
using KashTaskWPF.States;

namespace KashTaskWPF.Artifacts
{
    public sealed class PoisonedSpit : PoweredRenewableArtifact
    {
        private const int PERIOD = 2;
        private const double PERIODIC_DAMAGE = 5.0;
        public readonly int steps;
        public readonly double periodicDamage;
        public PoisonedSpit(int charge, int steps = PERIOD, double periodicDamage = PERIODIC_DAMAGE) : base(charge, "Ядовитая слюна")
        {
            this.steps = steps;
            this.periodicDamage = periodicDamage;
        }
        public PoisonedSpit(int charge): base(charge, "Ядовитая слюна")
        {
            this.steps = PERIOD;
        }
        protected sealed override void ActionsWithState(Character target)
        {
            PoisonedState stateToAdd = new PoisonedState(target, steps, periodicDamage);
            target.AddStateD(stateToAdd);
        }
        public sealed override double ConsumptionPerPower => 1;
    }
}
