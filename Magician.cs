using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game
{
    public class Magician : Character
    {
        private double mana;
        private double maxMana;
        private readonly Dictionary<short, Spell> spells = new Dictionary<short, Spell>();

        public Magician(string name, Race race, Sex sex, int age, double maxHealth, int experience, double maxMana): base(name, race, sex, age, maxHealth, experience)
        {
            this.maxMana = maxMana;
            this.mana = maxMana;
        }

        public double Mana
        {
            get
            {
                return mana;
            }
            set
            {
                if (value < 0 || value > MaxMana)
                {
                    throw new ArgumentException("Mana cannot be less then 0 or more than MaxMana");
                }
                mana = value;
            }
                
        }

        public double MaxMana
        {
            get
            {
                return maxMana;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Max mana cannot be less then zero");
                }
                maxMana = value;
            }
        }

        public int GetNumberOfSpells()
        {
            return spells.Count();
        }

        public Spell GetSpellById(short spellID)
        {
            return spells[spellID];
        }

        public void LearnSpell(Spell spell)
        {
            spells.Add(spell.GetClassID(), spell);
            //bool wasLearned = spells.Contains(spell);
            //if (wasLearned == true)
            //{
            //    return false;
            //}
            //spells.Add(spell);
            //return true;
        }

        public void ForgetSpell(Spell spell)
        {
            spells.Remove(spell.GetClassID());
            /*bool wasLearned = spells.Contains(spell);
            if (wasLearned == true)
            {
                spells.Remove(spell);
                return true;
            }            
            return false;*/
        }

        public bool UseSpell(Spell spell, Character another)
        {
            if (spells.ContainsKey(spell.GetClassID()))
            {
                spell.MagicEffect(this, another);
                return true;
            }
            return false;
            /*bool wasLearned = spells.Contains(spell);
            if (wasLearned == false)
            {
                return false;
            }
            spell.MagicEffect(this, another);
            return true;*/
        }

        public bool UseSpell(Spell spell)
        {
            if (spells.ContainsKey(spell.GetClassID()))
            {
                (spell as IMagic).MagicEffect(this);
                return true;
            }
            return false;
            /*bool wasLearned = spells.Contains(spell);
            if (wasLearned == false)
            {
                return false;
            }
            spell.MagicEffect(this);
            return true;*/
        }

        public bool UseSpell(Spell spell, Character another, double power)
        {
            if (spell is IMagicPowered)
            {
                if (spells.ContainsKey(spell.GetClassID()))
                {
                    (spell as IMagicPowered).MagicEffect(this, another, power);
                    return true;
                }
            }
            return false;
        }

        public bool UseSpell(Spell spell, double power)
        {
            if (spell is IMagicPowered)
            {
                if (spells.ContainsKey(spell.GetClassID()))
                {
                    (spell as IMagicPowered).MagicEffect(this, power);
                    return true;
                }
            }
            return false;
        }
    }
}
