using game;

namespace KashTaskWPF.Artifacts
{ 
    public abstract class Artifact : IMagic
    {
        protected Artifact(bool isRechargeable)
        {
            IsRechargeable = isRechargeable;
        }

        public bool IsRechargeable { get; }

        public string NAME { get; protected set; }

        public abstract void MagicEffect(Character user, Character target);
    }
}
