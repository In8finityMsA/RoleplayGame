using System;
using KashTaskWPF.Interface;

namespace KashTaskWPF.Artifacts
{ 
    public abstract class Artifact : IMagic, ICloneable
    {
        protected Artifact(bool isRechargeable)
        {
            IsRechargeable = isRechargeable;
        }

        public bool IsRechargeable { get; }

        public string NAME { get; protected set; }

        public abstract void MagicEffect(Character user, Character target);

        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
