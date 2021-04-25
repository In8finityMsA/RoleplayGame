using game;
using KashTaskWPF.States;

namespace KashTaskWPF.Spells
{
    internal abstract class AddStateSpell : Spell
    {
        protected AddStateSpell(double minManaCost, bool hasMotionalComponent, bool hasVerbalComponent, AbstractState stateToAdd, int steps) : base(minManaCost, hasMotionalComponent, hasVerbalComponent)
        {
            StateToAdd = stateToAdd;
            periodInSteps = steps;
        }

        public readonly int periodInSteps;
        public AbstractState StateToAdd { get; }

        public override void MagicEffect(Character user, Character target)
        {
            if (base.CheckCastRequirements(user))
            {
                target.AddStateD(StateToAdd);               
                ((Magician)user).Mana -= MinManaCost;               
            }
        }
    }
}
