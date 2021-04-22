﻿using game;

namespace KashTaskWPF.Spells
{
    internal abstract class RemoveStateSpell : Spell
    {
        protected RemoveStateSpell(double minManaCost, bool hasMotionalComponent, bool hasVerbalComponent, State stateToRemove) : base(minManaCost, hasMotionalComponent, hasVerbalComponent)
        {
            StateToRemove = stateToRemove;
        }

        public State StateToRemove { get; }

        public override void MagicEffect(Character user, Character target)
        {
            if (base.CheckCastRequirements(user))
            {
                if (target.RemoveState(StateToRemove))
                {
                    ((Magician)user).Mana -= MinManaCost;
                }
            }
        }

    }
}
