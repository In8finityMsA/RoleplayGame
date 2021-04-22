using game;

namespace KashTaskWPF.Spells
{
    internal abstract class AddStateSpell : Spell
    {
        protected AddStateSpell(double minManaCost, bool hasMotionalComponent, bool hasVerbalComponent, State stateToAdd) : base(minManaCost, hasMotionalComponent, hasVerbalComponent)
        {
            StateToAdd = stateToAdd;
        }

        public State StateToAdd { get; }

        public override void MagicEffect(Character user, Character target)
        {
            if (base.CheckCastRequirements(user))
            {
                if (target.AddState(StateToAdd))
                {
                    ((Magician)user).Mana -= MinManaCost;
                }
            }
        }

    }
}
