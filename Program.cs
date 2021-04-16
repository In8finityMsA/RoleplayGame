﻿using game;
using System;

namespace KashTask
{
    class Program
    {
        static void Main(string[] args)
        {
            //Magician Artas = new Magician("Artas", Race.HUMAN, Gender.MALE, 30, 900, 400);
            Magician Kenarius = new Magician("Kenarius", Race.ELF, Sex.MALE, 100000, 4000, 40, 40);
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
        }
    }
}
