namespace game
{
    class Heal : RemoveStateSpell
    {
        private const double MANA_COST = 30.0;
        public Heal() : base(MANA_COST, false, true, State.SICK)
        {
           NAME = "Вылечить";
        }
    }
}
