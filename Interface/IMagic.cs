namespace KashTaskWPF.Interface
{
    public interface IMagic
    {
        void MagicEffect(Character user, Character target);

        public void MagicEffect(Character user) => MagicEffect(user, user);        
    }
}