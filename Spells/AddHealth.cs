namespace game
{
    public class AddHealth : Spell, IMagicPowered
    {        
        private const double MANA_COST = 2.0; //mana needed per one health point
        public double ConsumptionPerPower { get; }

        public AddHealth() : base(MANA_COST, false, true)
        {
            ConsumptionPerPower = MANA_COST;
            NAME = "Добавить здоровье";
        }

        public override void MagicEffect(Character user, Character target)
        {
            MagicEffect(user, target, target.MaxHealth - target.Health);
        }

        public void MagicEffect(Character user, Character target, double power)
        {
            if (base.CheckCastRequirements(user) && target.StateHealth != StateHealth.DEAD)
            {
                Magician magicUser = (Magician) user;
                if (power > target.MaxHealth - target.Health)
                {
                    power = target.MaxHealth - target.Health;
                }

                if (magicUser.Mana >= power * ConsumptionPerPower)
                {
                    magicUser.Mana -= power * ConsumptionPerPower;
                    target.Health += power;
                }
                else
                {
                    target.Health += magicUser.Mana / ConsumptionPerPower;
                    magicUser.Mana = 0;
                }
            }
        }   
    }
}