using game;

namespace KashTaskWPF.Artifacts
{
    public sealed class Thunder : PoweredRenewableArtifact
    {
        public Thunder(int charge) : base(charge) 
        {
            NAME = $"Посох молния ({Charge})";
        }
        protected sealed override void ActionsWithState(Character target) { }
        public sealed override double ConsumptionPerPower => 1;
    }
}
