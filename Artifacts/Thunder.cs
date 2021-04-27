using game;

namespace KashTaskWPF.Artifacts
{
    public sealed class Thunder : PoweredRenewableArtifact
    {
        public Thunder(int charge) : base(charge, "Посох молния") 
        {
        }
        protected sealed override void ActionsWithState(Character target) { }
        public sealed override double ConsumptionPerPower => 1;
    }
}
