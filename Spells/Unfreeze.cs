namespace game
{
    class Unfreeze : RemoveStateSpell
    {
        private const double MANA_COST = 85.0;

        public Unfreeze() : base(MANA_COST, true, true, State.PARALIZED) {}    
    }
}
