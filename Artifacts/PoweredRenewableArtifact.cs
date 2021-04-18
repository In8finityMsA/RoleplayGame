using game;
using System;

namespace Artifacts
{
    public class NegativeChargeException : Exception
    {
        public NegativeChargeException(string message = "") : base(message) { }
    }

    public abstract class PoweredRenewableArtifact: Artifact, IMagicPowered
    {
        protected double charge;
        public PoweredRenewableArtifact(int charge): base(true)
        {
            Charge = charge;
        }

        public double Charge
        {
            get => charge;
            set
            {
                if (value < 0)
                {
                    throw new NegativeChargeException("Charge can't be less than 0.");
                }
                charge = value;
            }
        }

        public abstract double ConsumptionPerPower { get; }

        protected abstract void ActionsWithState(Character target);

        public sealed override void MagicEffect(Character user, Character target)
        {
            MagicEffect(user, target, target.MaxHealth - target.Health);
        }

        public void MagicEffect(Character user, Character target, double power)
        {                          
            if (power <= Charge)
            {
                target.Health -= power;
                Charge -= power;
                ActionsWithState(target);
             }
            else if (Charge != 0)
            {
                target.Health -= Charge;
                Charge = 0;
                ActionsWithState(target);
            }                       
        }
    }
}
