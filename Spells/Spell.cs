using System;
using game;

namespace KashTaskWPF.Spells
{
    public abstract class Spell : IMagic
    {        
        protected Spell(double minManaCost, bool hasMotionalComponent, bool hasVerbalComponent)
        {
            if (minManaCost < 0)
            {
                throw new ArgumentException("MinManaCost cannot be less than zero.");
            }

            MinManaCost = minManaCost;
            HasMotionalComponent = hasMotionalComponent;
            HasVerbalComponent = hasVerbalComponent;  
       
        }

        //public abstract short GetClassID();

        public bool HasMotionalComponent { get; }
        public bool HasVerbalComponent { get; }
        public double MinManaCost { get; }

        public string NAME { get; protected set; }

        //Pavel: Definition for MagicEffect(user) is in interface default implementation.
        //Pavel: Maybe move it here to not abstract method. But that will be not consistent with IMagicPowered
        public abstract void MagicEffect(Character user, Character target);

        //Pavel: Does it need to be public? To check possibility to cast a spell from other classes.
        protected bool CheckCastRequirements(Character user) 
        {
            if ((!HasMotionalComponent || user.CanMoveNow) && (!HasVerbalComponent || user.CanSpeakNow) && (user is Magician magicUser)) {
                if (magicUser.Mana >= MinManaCost)
                {
                    return true;
                }
                else
                {
                    throw new NotEnoughManaException();
                }
            }
            else
            {
                throw new NotPerformableActionException();
            }
        }

        public class NotEnoughManaException : Exception
        {
            public NotEnoughManaException(string message = "")
                : base(message)
            {
            }
        }

        public class NotPerformableActionException : Exception
        {
            public NotPerformableActionException(string message = "")
                : base(message)
            {
            }
        }
    }
}