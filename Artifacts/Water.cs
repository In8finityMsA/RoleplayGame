using System;

namespace KashTaskWPF.Artifacts
{
    public enum BottleSize
    {
        S = 10, M = 25, L = 50
    }

    public abstract class Water: Artifact
    {
        protected Water(BottleSize size) : base(false)
        {
            Size = size;
        }

        protected Water(int size) : base(false)
        {
            foreach (BottleSize bottleSize in Enum.GetValues(typeof(BottleSize)))
            {
                if ((BottleSize) size == bottleSize)
                {
                    Size = (BottleSize) size;
                    return;
                }
            }

            throw new ArgumentException($"No such value in enum BottleSize - {size}");
        }

        public BottleSize Size { get; protected set; }

        public sealed override void MagicEffect(Character user, Character target)
        {            
            DrinkAction(target);
            user.RemoveArtifact(this);                      
        }
        protected abstract void DrinkAction(Character target);
    }
}
