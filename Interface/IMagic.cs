namespace game
{
    public interface IMagic
    {
        void MagicEffect(Magician user, Character target);

        public void MagicEffect(Magician user) => MagicEffect(user, user);
        
    }
}