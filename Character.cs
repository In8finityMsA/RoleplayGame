using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game
{   
    public enum StateHealth
    {
        NORMAL, WEAK, DEAD
    }

    public enum State
    {
        SICK, POISONED, PARALIZED
    }

    public enum Race
    {
        HUMAN, GNOME, ELF, ORC, GOBLIN
    }

    public enum Sex
    {
        MALE, FEMALE
    }

    public class Character: IComparable
    {
        private static int unique_ID = 0;

        private readonly string name;
        private readonly int ID;
        private readonly Race race;
        private readonly Sex sex;
              
        private int age;
        private double health;
        private StateHealth stateHealth;
        private double maxHealth;
        private int experience;

        private bool canSpeakNow = false;
        private bool canMoveNow = false;

        private readonly List<Artifact> inventory = new List<Artifact>();        
        public int GetNumberOfArtifacts()
        {
            return inventory.Count();
        }

        public Artifact GetArtifactByIndex(int index)
        {
            return inventory[index];
        }

        

        public Character(string name, Race race, Sex sex): this(name, race, sex, 0, 100, 0)
        {
            
        }

        //optional
        public Character(string name, Race race, Sex sex, int age) : this(name, race, sex, age, 100, 0)
        {

        }
        //optional
        public Character(string name, Race race, Sex sex, int age, double maxHealth) : this(name, race, sex, age, maxHealth, 0)
        {
            
        }
        //optional
        public Character(string name, Race race, Sex sex, int age, double maxHealth, int experience)
        {
            this.ID = ++unique_ID;
            this.name = name;
            this.race = race;
            this.sex = sex;
            this.age = age;
            this.maxHealth = maxHealth;
            this.health = maxHealth;
            this.stateHealth = StateHealth.NORMAL;
            this.experience = experience;
        }       

        public int Age 
        { 
          get
            {
                return age;
            }
          private set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Age cannot be less than zero");
                }
            }
        }

        public double Health
        {
            get
            {
                return health;
            }
            set
            {
                if (value < 0 || value > maxHealth)
                {
                    throw new ArgumentException("Health cannot be less than 0 or more than maxHealth");
                }
                health = value;
                ManageState();
            }
        }

        public double MaxHealth
        {
            get
            {
                return maxHealth;

            }
        }

        public StateHealth State
        {
            get
            {
                return stateHealth;
            }
            private set
            {
                if (stateHealth != StateHealth.DEAD)
                {
                    stateHealth = value;
                }
                else
                {
                    throw new Exception("State cannot be changed if character is already DEAD");
                }
            }
        }

        public Sex Sex
        {
            get
            {
                return sex;
            }                
        }

        public int Experience
        {
            get
            {
                return experience;
            }
            private set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Experience cannot be less than 0");
                }
            }
        }

        public bool CanSpeakeNow
        {
            get
            {
                return canSpeakNow;
            }
            set
            {
                canSpeakNow = value;
            }
        }

        public bool CanMoveNow
        {
            get
            {
                return canSpeakNow;
            }
            set
            {
                canMoveNow = value;
            }                
        }

        public int CompareTo(object obj)
        {
            if (obj is Character)
            {
                return experience.CompareTo(((Character)obj).experience);
            }
            throw new ArgumentException("obj is not type of Character");
        }

        public void ManageState()
        {
            if (health <= 0.1 * maxHealth)
            {
                stateHealth = StateHealth.WEAK;
            }
            else if (health >= 0.1 * maxHealth)
            {
                stateHealth = StateHealth.NORMAL;
            }
            else if (health <= 0)
            {
                stateHealth = StateHealth.DEAD;
            }
        }

        

        public override string ToString()
        {
            string s1 = "ID: " + this.ID + "\n" + "Name: " + this.name + "\n" + "Race: " + this.race + "\n";
            string s2 = "Sex: " + this.race + "\n" + "Age:" + this.age + "\n" + "Health: " + this.health + "\n";
            string s3 = "State: " + this.stateHealth + "\n" + "Max health: " + this.maxHealth + "\n";
            string s4 = "Ability to speak now: " + this.canSpeakNow + "\n";
            string s5 = "Ability to move now: " + this.canMoveNow;
            return s1 + s2 + s3 + s4 + s5;   
        }

        public void PickUpArtifact(Artifact artifact)
        {
            inventory.Add(artifact);
        }

        public bool RemoveArtifactFromInventary(Artifact artifact)
        {
            return inventory.Remove(artifact);
        }

        public bool GiveArtifactToAnotherCharacter(Character another, Artifact artifact)
        {
            bool wasInInventary = inventory.Remove(artifact);
            if (wasInInventary == true)
            {
                another.inventory.Add(artifact);
                return true;
            }
            return false;
        }

        public bool UseArtifact(Artifact artifact, Character another)
        {
            bool wasInInventary = inventory.Remove(artifact);
            if (wasInInventary == true)
            {
                artifact.MagicAction(this, another);
                return true;
            }
            return false;
        }

        public bool UseArtifact(Artifact artifact)
        {
            bool wasInInventary = inventory.Remove(artifact);
            if (wasInInventary == true)
            {
                artifact.MagicAction(this);
                return true;
            }
            return false;
        }


    }
    //interface IMagic
    //{
    //    void MagicAction();
    //    void MagicAction(Character from, Character to);
    //    void MagicAction(Character from);
    //}



    //class Spell : IMagic
    //{
    //    public void MagicAction()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void MagicAction(Character from, Character to)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void MagicAction(Character from)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}



    //class Artifact : IMagic
    //{
    //    public void MagicAction(Character from)
    //    {
    //        throw new NotImplementedException();
    //    }
    //    public void MagicAction(Character from, Character to)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public void MagicAction()
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
