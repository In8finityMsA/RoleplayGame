using game;
using System;

namespace KashTaskWPF.Artifacts
{
    public class NegativeChargeException : Exception
    {
        public NegativeChargeException(string message = "") : base(message) { }
    }

    public abstract class PoweredRenewableArtifact: Artifact, IMagicPowered
    {
        protected double charge;
        public PoweredRenewableArtifact(int charge, string shortName): base(true)
        {
            this.shortName = shortName;
            Charge = charge;
            NAME = shortName + $" ({Charge})";
        }

        public double Charge
        {
            get => charge;
            set
            {
                if (value < 0)
                {
                    charge = 0;
                }
                charge = value;
            }
        }

        protected string shortName;

        public abstract double ConsumptionPerPower { get; }

        protected abstract void ActionsWithState(Character target);

        public sealed override void MagicEffect(Character user, Character target)
        {
            MagicEffect(user, target, target.MaxHealth - target.Health);
        }

        public void MagicEffect(Character user, Character target, double power)
        {
            double fee = power * ConsumptionPerPower;
            if (fee <= Charge)
            {
                ActionsWithState(target); 
                target.Health -= fee;
                Charge -= fee;
                
            }
            else if (Charge != 0)
            {
                ActionsWithState(target); 
                target.Health -= Charge;
                Charge = 0;
            }

            NAME = shortName + $" ({Charge})";
        }
    }
}
