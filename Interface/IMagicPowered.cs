namespace game
{
    public interface IMagicPowered : IMagic
    {
        void MagicEffect(Character user, Character target, double power);

        void MagicEffect(Character user, double power) => MagicEffect(user, user, power);

        double ConsumptionPerPower { get; }
    }
}
