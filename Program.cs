using game;
using System;

namespace KashTask
{
    class Program
    {
        static void Main(string[] args)
        {
            Magician Artas = new Magician("Artas", Race.HUMAN, Sex.MALE, 30, 900, 400, 200000);
            Magician Kenarius = new Magician("Kenarius", Race.ELF, Sex.MALE, 100000, 4000, 40, 40);
            Kenarius.AddState(State.PARALIZED);
            Console.WriteLine(Kenarius.ToString());
            Character Thunder = new Character("Thunder", Race.ORC, Sex.MALE, 40, 2700);
            Thunder.Health = 850;
           
            Console.WriteLine(Thunder.Health);
            Console.WriteLine("**");
            Console.WriteLine(Kenarius.Mana);

            AddHealth addHealth = new AddHealth();
            addHealth.MagicEffect(Kenarius, Thunder, 1850);
            Console.WriteLine("----------------------------------------------");

            Console.WriteLine(Thunder.Health);
            Console.WriteLine("**");
            Console.WriteLine(Kenarius.Mana);

            Unfreeze rev = new Unfreeze();
            Artas.LearnSpell((Spell)rev);
            Artas.UseSpell(rev, Kenarius);
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine(Kenarius.ToString());
            Console.WriteLine("----------------------------------------------");
            Console.WriteLine(Artas.ToString());
        }
    }
}
