using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game
{ 
    public abstract class Artifact : IMagic
    {
        public Artifact(bool isRechargable, double charge)
        {
            IsRechargable = isRechargable;
            Charge = charge;
        }

        public abstract short GetClassID();

        public bool IsRechargable { get; }
        public double Charge { get; protected set; }

        public abstract void MagicEffect(Magician user, Character target);

        protected void Use(double charge)
        {
            Charge -= charge;
            if (Charge <= 0 && !IsRechargable)
            {
                //this.
            }
        }

        /*protected bool CheckUseRequirements()
        {
            if 
        }*/
    }
}
