namespace game
{ 
    public abstract class Artifact : IMagic
    {
        public Artifact(bool isRechargable)
        {
            IsRechargable = isRechargable;
        }

        public bool IsRechargable { get; }

        public abstract void MagicEffect(Character user, Character target);
    }
}
