using KashTaskWPF.States;

namespace KashTaskWPF.Spells
{
    internal abstract class AddStateSpell : Spell
    {
        protected AddStateSpell(double minManaCost, bool hasMotionalComponent, bool hasVerbalComponent) : base(minManaCost, hasMotionalComponent, hasVerbalComponent) { }

        public abstract AbstractState CreateState(Character carrier, int steps);

        public override void MagicEffect(Character user, Character target)
        {
            if (base.CheckCastRequirements(user) && target.StateHealth != StateHealth.DEAD)
            {
                Magician magicUser = (Magician)user;

                if (magicUser.Mana >= 1 * MinManaCost)
                {
                    magicUser.Mana -= 1 * MinManaCost;
                    target.AddStateD(CreateState(target, 1));
                }
            }
        }
    }
}
