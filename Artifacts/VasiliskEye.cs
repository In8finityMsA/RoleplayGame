using KashTaskWPF.States;

namespace KashTaskWPF.Artifacts
{
    public sealed class VasiliskEye : Artifact
    {
        private const int PERIOD = 2;
        public readonly int steps;
        public VasiliskEye(int steps) : base(false) 
        {
            NAME = "Глаз василиска";
            this.steps = steps;
        }
        
        public VasiliskEye() : base(false)
        {
            NAME = "Глаз василиска";
            steps = PERIOD;
        }
        
        public sealed override void MagicEffect(Character user, Character target)
        {
            ParalizedState stateToAdd = new ParalizedState(target, steps);
            target.AddStateD(stateToAdd);
            user.RemoveArtifact(this);
        }
    }
}
