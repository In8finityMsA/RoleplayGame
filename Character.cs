using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game
{   
    enum STATE
    {
        NORMAL, WEAK, /*SICK, POISONED, PARALIZED,*/ DEAD
    }

    enum RACE
    {
        HUMAN, GNOME, ELF, ORC, GOBLIN
    }

    enum SEX
    {
        MALE, FEMALE
    }

    class Character: IComparable
    {
        private static int unic_ID = 0;

        private readonly string name;
        private readonly int ID;
        private readonly RACE race;
        private readonly SEX sex;
              
        private int age;
        private int health;
        private STATE state;
        private int maxHealth;
        private int experience;

        private bool isAbleToSpeakNow = false;
        private bool isAbleToMoveNow = false;

        private readonly List<Artifact> inventary = new List<Artifact>(); 
        public int GetNumberOfArtifacts()
        {
            return inventary.Count();
        }

        public Artifact GetArtifactByIndex(int index)
        {
            return inventary[index];
        }

        public Character(string name, RACE race, SEX sex): this(name, race, sex, 0, 100, 0)
        {
            
        }

        //optional
        public Character(string name, RACE race, SEX sex, int age) : this(name, race, sex, age, 100, 0)
        {

        }
        //optional
        public Character(string name, RACE race, SEX sex, int age, int maxHealth) : this(name, race, sex, age, maxHealth, 0)
        {
            
        }
        //optional
        public Character(string name, RACE race, SEX sex, int age, int maxHealth, int experience)
        {
            this.ID = ++unic_ID;
            this.name = name;
            this.race = race;
            this.sex = sex;
            this.age = age;
            this.maxHealth = maxHealth;
            this.health = maxHealth;
            this.state = STATE.NORMAL;
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

        public int Health
        {
            get
            {
                return health;
            }
            private set
            {
                if (value < 0 || value > maxHealth)
                {
                    throw new ArgumentException("Health cannot be less than 0 or more than maxHealth");
                }
                health = value;
                ManageState();
            }
        }

        public STATE State
        {
            get
            {
                return state;
            }
            private set
            {
                if (state != STATE.DEAD)
                {
                    state = value;
                }
                else
                {
                    throw new Exception("State cannot be changed if character is already DEAD");
                }
            }
        }

        public SEX Sex
        {
            get
            {
                return sex;
            }                
        }

        public int MaxHealth
        {
            get
            {
                return maxHealth;
                    
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

        public bool IsAbleToSpeakeNow
        {
            get
            {
                return isAbleToSpeakNow;
            }
            set
            {
                isAbleToSpeakNow = value;
            }
        }

        public bool IsAbleToMoveNow
        {
            get
            {
                return isAbleToSpeakNow;
            }
            set
            {
                isAbleToMoveNow = value;
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
                state = STATE.WEAK;
            }
            else if (health >= 0.1 * maxHealth)
            {
                state = STATE.NORMAL;
            }
            else if (health == 0)
            {
                state = STATE.DEAD;
            }
        }

        

        public override string ToString()
        {
            string s1 = "ID: " + this.ID + "\n" + "Name: " + this.name + "\n" + "Race: " + this.race + "\n";
            string s2 = "Sex: " + this.race + "\n" + "Age:" + this.age + "\n" + "Health: " + this.health + "\n";
            string s3 = "State: " + this.state + "\n" + "Max health: " + this.maxHealth + "\n";
            string s4 = "Ability to speak now: " + this.isAbleToSpeakNow + "\n";
            string s5 = "Ability to move now: " + this.isAbleToMoveNow;
            return s1 + s2 + s3 + s4 + s5;   
        }

        public void PickUpArtifact(Artifact artifact)
        {
            inventary.Add(artifact);
        }

        public bool RemoveArtifactFromInventary(Artifact artifact)
        {
            return inventary.Remove(artifact);
        }

        public bool GiveArtifactToAnotherCharacter(Character another, Artifact artifact)
        {
            bool wasInInventary = inventary.Remove(artifact);
            if (wasInInventary == true)
            {
                another.inventary.Add(artifact);
                return true;
            }
            return false;
        }

        public bool UseArtifact(Artifact artifact, Character another)
        {
            bool wasInInventary = inventary.Remove(artifact);
            if (wasInInventary == true)
            {
                artifact.MagicAction(this, another);
                return true;
            }
            return false;
        }

        public bool UseArtifact(Artifact artifact)
        {
            bool wasInInventary = inventary.Remove(artifact);
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
