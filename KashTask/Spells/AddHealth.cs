namespace game
{
    public class AddHealth : Spell, IMagicPowered
    {
        private const double MANA_PER_HEALTH = 2.0f; //mana needed per one health point
        public override void MagicEffect(Magician user, Character target)
        {
            MagicEffect(user, target, target.MaxHealth - target.Health);
        }

        public override void MagicEffect(Magician user)
        {
            MagicEffect(user, user);
        }


        public void MagicEffect(Magician user, Character target, double power)
        {
            if (power > target.MaxHealth - target.Health)
            {
                power = target.MaxHealth - target.Health;
            }

            if (user.Mana > power * MANA_PER_HEALTH)
            {
                user.Mana -= power * MANA_PER_HEALTH;
                target.Health += power;
            }
            else
            {
                target.Health = user.Mana * MANA_PER_HEALTH;
                user.Mana = 0;
            }
        }
        public void MagicEffect(Magician user, double power) 
        {
            MagicEffect(user, user, power);
        }
        

        
    }
}