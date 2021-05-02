namespace KashTaskWPF.Spells
{
    class Revive : Spell
    {
        private const double MANA_COST = 150.0;

        public Revive() : base(MANA_COST, true, true) 
        {
            NAME = "Оживить";
        }

        public override void MagicEffect(Character user, Character target)
        {
            if (base.CheckCastRequirements(user))
            {
                if (target.StateHealth == StateHealth.DEAD)
                {
                    target.Revive();
                }
            }
        }
    }
}
