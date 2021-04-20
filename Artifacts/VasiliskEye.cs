using game;

namespace Artifacts
{
    public sealed class VasiliskEye : Artifact
    {
        public VasiliskEye() : base(false) { }
        
        public override sealed void MagicEffect(Character user, Character target)
        {            
            target.AddState(State.PARALIZED);
            user.RemoveArtifact(this);
            NAME = "Глаз василиска";
        }
    }
}
