namespace game
{
    public abstract class Spell : IMagic
    {
        private bool hasMotionalComponent;
        private bool hasVerbalComponent;
        private double minManaCost;

        public abstract void MagicEffect(Magician user, Character target);
        public abstract void MagicEffect(Magician user);
    }
}