namespace game
{
    public interface IMagicPowered : IMagic
    {
        void MagicEffect(Magician user, Character target, double power);

        void MagicEffect(Magician user, double power)
        {
            MagicEffect(user, user, power);
        }

        double ManaPerPower { get; }
    }
}
