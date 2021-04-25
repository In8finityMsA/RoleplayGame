using game;

namespace KashTaskWPF.Artifacts
{
    class FrogLegDecoct : Artifact
    {
        public FrogLegDecoct() : base(false) 
        {
            NAME = "Декокт из лягушачьих лапок";
        }

        public override void MagicEffect(Character user, Character target)
        {           
            target.RemoveStateD(State.POISONED);
            user.RemoveArtifact(this);         
        }
    }
}
