namespace game
{
    public class AddHealth : Spell, IMagicPowered
    {
        public static readonly short SpellID = SpellIDManager.GetNextID();
        public override short GetClassID()
        {
            return SpellID;
        }

        private const double MANA_COST = 2.0; //mana needed per one health point
        public double ConsumptionPerPower { get; }

        public AddHealth() : base(MANA_COST, false, true)
        {
            ConsumptionPerPower = MANA_COST;
        }

        public override void MagicEffect(Magician user, Character target)
        {
            MagicEffect(user, target, target.MaxHealth - target.Health);
        }

        public void MagicEffect(Magician user, Character target, double power)
        {
            if (base.CheckCastRequirements(user) && target.StateHealth != StateHealth.DEAD)
            {
                if (power > target.MaxHealth - target.Health)
                {
                    power = target.MaxHealth - target.Health;
                }

                if (user.Mana >= power * ConsumptionPerPower)
                {
                    user.Mana -= power * ConsumptionPerPower;
                    target.Health += power;
                }
                else
                {
                    target.Health += user.Mana / ConsumptionPerPower;
                    user.Mana = 0;
                }
            }
        }
   
    }
}