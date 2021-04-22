using game;

namespace KashTaskWPF.Spells
{
    class Antidote : RemoveStateSpell
    {
        private const double MANA_COST = 20.0;
        public Antidote() : base(MANA_COST, false, true, State.POISONED) 
        {
            NAME = "Противоядие";
        }

    }
}
