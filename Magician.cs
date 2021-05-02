using KashTaskWPF.Artifacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KashTaskWPF;
using KashTaskWPF.Interface;
using KashTaskWPF.Spells;

namespace KashTaskWPF
{
    public class Magician : Character
    {
        private double mana;
        private double maxMana;
        private readonly Dictionary<Type, Spell> spells = new Dictionary<Type, Spell>();

        public Magician(string name, Race race, Sex sex, int age, double maxHealth, int experience, double maxMana): base(name, race, sex, age, maxHealth, experience)
        {
            this.maxMana = maxMana;
            this.mana = maxMana;
        }

        public Magician(Magician other): base(other)
        {
            mana = other.mana;
            maxMana = other.maxMana;
            spells = other.spells.ToDictionary(entry => entry.Key,
                entry => entry.Value);//just copy
        }

        public double Mana
        {
            get => mana;          
            set
            {
                if (value < 0 || value > MaxMana)
                {
                    throw new ArgumentException("Mana cannot be less then 0 or more than MaxMana.");
                }
                mana = value;
            }                
        }

        public double MaxMana
        {
            get => maxMana;           
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Max mana cannot be less then zero.");
                }
                maxMana = value;
            }
        }

        public Dictionary<Type, Spell> Spells { get => spells; }

        public int GetNumberOfSpells() => spells.Count;

        public Spell GetSpellById(Type type) => spells[type];

        public void LearnSpell(Spell spell)
        {
            base.CheckIfDeadTryAct();
            spells.Add(spell.GetType(), spell);
        }

        public void LearnSpell(Type type)
        {
            base.CheckIfDeadTryAct();
            Spell spell = Activator.CreateInstance(type) as Spell;
            if (spell != null)
            {
                LearnSpell(spell);
            }
        }

        public void ForgetSpell(Type type) 
        {
            base.CheckIfDeadTryAct();
            spells.Remove(type);
        }

        public void ForgetSpell(Spell spell)
        {
            base.CheckIfDeadTryAct();
            spells.Remove(spell.GetType());
        }

        public bool UseSpell(Spell spell, Character another)
        {
            base.CheckIfDeadTryAct();
            if (spells.ContainsKey(spell.GetType()))
            {
                spell.MagicEffect(this, another);
                return true;
            }
            return false;         
        }

        public bool UseSpell(Type type, Character another)
        {
            base.CheckIfDeadTryAct();
            Spell spell = Activator.CreateInstance(type) as Spell;
            if (spell != null)
            {
                return UseSpell(spell, another);
            }
            return false;
        }

        public bool UseSpell(Spell spell)
        {
            base.CheckIfDeadTryAct();
            if (spells.ContainsKey(spell.GetType()))
            {
                (spell as IMagic).MagicEffect(this);
                return true;
            }
            return false;            
        }

        public bool UseSpell(Type type)
        {
            base.CheckIfDeadTryAct();
            Spell spell = Activator.CreateInstance(type) as Spell;
            if (spell != null)
            {
                return UseSpell(spell);
            }
            return false;
        }

        public bool UseSpell(Spell spell, Character another, double power)
        {
            base.CheckIfDeadTryAct();
            if (spell is IMagicPowered castedSpell)
            {
                if (spells.ContainsKey(spell.GetType()))
                {
                    castedSpell.MagicEffect(this, another, power);
                    return true;
                }
            }
            return false;
        }

        public bool UseSpell(Type type, Character another, double power)
        {
            base.CheckIfDeadTryAct();
            Spell spell = Activator.CreateInstance(type) as Spell;
            if (spell != null)
            {
                return UseSpell(spell, another, power);
            }
            return false;
        }

        public bool UseSpell(Spell spell, double power)
        {
            base.CheckIfDeadTryAct();
            if (spell is IMagicPowered castedSpell)
            {
                if (spells.ContainsKey(spell.GetType()))
                {
                    castedSpell.MagicEffect(this, power);
                    return true;
                }
            }
            return false;
        }

        public bool UseSpell(Type type, double power)
        {
            base.CheckIfDeadTryAct();
            Spell spell = Activator.CreateInstance(type) as Spell;
            if (spell != null)
            {
                return UseSpell(spell, power);
            }
            return false;
        }

        public override string ToString()
        {
            return base.ToString() + "\n" +
                "Мана: " + Mana + "\n" + "Максимальная мана: " + MaxMana + "\n" +
                "Заклинания: " + GetSpellsNames();
        }

        private string GetSpellsNames()
        {
            string str = "";
            foreach (var item in spells.Keys)
            {
                str += item.Name + " ";
            }
            return str;
        }

        public void ChargeArtifact(PoweredRenewableArtifact artifact, int power)
        {
            CheckIfDeadTryAct();
            if (power <= Mana)
            {
                Mana -= power;
                artifact.Charge += power;
            }
            else
            {
                artifact.Charge += Mana;
                Mana = 0;
            }
        }
    }
}
