using game;

namespace Artifacts
{
    class FrogLegDecoct : Artifact
    {
        public FrogLegDecoct() : base(false) 
        {
            NAME = "Декокт из лягушачьих лапок";
        }

        public override void MagicEffect(Character user, Character target)
        {           
            target.RemoveState(State.POISONED);
            user.RemoveArtifact(this);         
        }
    }
}
