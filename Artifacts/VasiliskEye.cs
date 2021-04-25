using game;
using KashTaskWPF.States;

namespace KashTaskWPF.Artifacts
{
    public sealed class VasiliskEye : Artifact
    {
        private const int period = 5;
        public readonly int steps;
        public VasiliskEye(int steps = period) : base(false) 
        {
            NAME = "Глаз василиска";
            this.steps = steps;
        }
        
        public override sealed void MagicEffect(Character user, Character target)
        {
            ParalizedState stateToAdd = new ParalizedState(target, steps);
            target.AddStateD(stateToAdd);
            user.RemoveArtifact(this);
        }
    }
}
