using game;

namespace Artifacts
{
    class VasiliskEye : Artifact
    {
        public VasiliskEye() : base(false) { }
        
        public override void MagicEffect(Character user, Character target)
        {            
            target.AddState(State.PARALIZED);
            user.RemoveArtifact(this);                       
        }
    }
}
