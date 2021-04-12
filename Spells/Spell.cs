using System;

namespace game
{
    public abstract class Spell : IMagic
    {       
        public Spell(double minManaCost, bool hasMotionalComponent, bool hasVerbalComponent)
        {
            if (minManaCost < 0)
            {
                throw new ArgumentException("MinManaCost cannot be less than zero");
            }

            MinManaCost = minManaCost;
            HasMotionalComponent = hasMotionalComponent;
            HasVerbalComponent = hasVerbalComponent;            
        }

        public bool HasMotionalComponent { get; }
        public bool HasVerbalComponent { get; }
        public double MinManaCost { get; }      

        public abstract void MagicEffect(Magician user, Character target);
        /*public abstract void MagicEffect(Magician user);*/ //Left it default form defined in interface. Or move default form here?

        protected bool CheckCastRequirements(Magician user) //Does it need to be public? To check from other classes possibility to cast a spell
        {
            return (!HasMotionalComponent || user.CanMoveNow) && (!HasVerbalComponent || user.CanSpeakeNow) && (user.Mana >= MinManaCost);
        }
    }
}