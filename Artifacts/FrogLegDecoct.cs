using game;

namespace Artifacts
{
    class FrogLegDecoct : Artifact
    {
        public FrogLegDecoct() : base(false) { }

        public override void MagicEffect(Character user, Character target)
        {           
            target.RemoveState(State.POISONED);
            user.RemoveArtifact(this);         
        }
    }
}
