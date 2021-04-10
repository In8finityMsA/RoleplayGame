using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game
{
    class MagicUser : Character
    {
        private int mana;
        private int maxMana;
        private readonly List<Spell> spells = new List<Spell>();

        public MagicUser(string name, RACE race, SEX sex, int age, int maxHealth, int experience, int maxMana): base(name, race, sex, age, maxHealth, experience)
        {
            this.maxMana = maxMana;
            this.mana = maxMana;
        }

        public int Mana
        {
            get
            {
                return mana;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Mana cannot be less then zero");
                }
                mana = value;
            }
                
        }

        public int MaxMana
        {
            get
            {
                return maxMana;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Max Mana cannot be less then zero");
                }
                maxMana = value;
            }
        }

        public int NumberOfSpells()
        {
            return spells.Count();
        }

        public Spell GetSpellByIndex(int index)
        {
            return spells[index];
        }

        public bool LearnSpell(Spell spell)
        {
            bool wasLearned = spells.Contains(spell);
            if (wasLearned == true)
            {
                return false;
            }
            spells.Add(spell);
            return true;
        }

        public bool ForgetSpell(Spell spell)
        {
            bool wasLearned = spells.Contains(spell);
            if (wasLearned == true)
            {
                spells.Remove(spell);
                return true;
            }            
            return false;
        }

        public bool UseSpell(Spell spell, Character another)
        {
            bool wasLearned = spells.Contains(spell);
            if (wasLearned == false)
            {
                return false;
            }
            spell.MagicAction(this, another);
            return true;
        }

        public bool UseSpell(Spell spell)
        {
            bool wasLearned = spells.Contains(spell);
            if (wasLearned == false)
            {
                return false;
            }
            spell.MagicAction(this);
            return true;
        }
    }
}
