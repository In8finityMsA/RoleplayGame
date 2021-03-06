using game;

namespace Artifacts
{
    public enum BottleSize
    {
        S = 10, M = 25, L = 50
    }

    public abstract class Water: Artifact
    {
        private BottleSize size;

        public Water(BottleSize size) : base(false)
        {
            this.size = size;
        }

        public BottleSize Size { get => size; }

        public sealed override void MagicEffect(Character user, Character target)
        {            
            ActionsWithHealth(target);
            user.RemoveArtifact(this);                      
        }
        protected abstract void ActionsWithHealth(Character target);
    }
}
