using Artifacts;
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

    public class DeadTryToactException: Exception
    {
        public DeadTryToactException(string message = "") : base(message) { }
    }
        
    public class Character: IComparable
    {
        private static int unique_ID = 0;

        private readonly string name;
        private readonly int id;
        private readonly Race race;
        private readonly Sex sex;
              
        private int age;
        private double health;
        private StateHealth stateHealth;
        private HashSet<State> states = new HashSet<State>();
        private double maxHealth;
        private int experience;

        private bool canSpeakNow = true;
        private bool canMoveNow = true;

        private readonly List<Artifact> inventory = new List<Artifact>();        

        public Character(string name, Race race, Sex sex) : this(name, race, sex, 0, 100, 0) { }

        public Character(string name, Race race, Sex sex, int age) : this(name, race, sex, age, 100, 0) { }

        public Character(string name, Race race, Sex sex, int age, double maxHealth) : this(name, race, sex, age, maxHealth, 0) { }

        public Character(string name, Race race, Sex sex, int age, double maxHealth, int experience)
        {
            this.id = ++unique_ID;
            this.name = name;
            this.race = race;
            this.sex = sex;
            Age = age;
            MaxHealth = maxHealth;
            health = maxHealth;
            StateHealth = StateHealth.NORMAL;
            Experience = experience;
        }       
        
        public int ID { get => id; }
        public string Name { get => name; }
        public Race Race { get => race; }
        public Sex Sex { get => sex; }

        public int Age
        {
            get => age;
            set  
            {
                if (value < 0)
                {
                    throw new ArgumentException("Age cannot be less than zero.");
                }
                age = value;
            }
        }

        public double Health
        {
            get => health;            
            set
            {
                if (health != 0)
                {
                    if (value < 0 || value > maxHealth)
                    {
                        throw new ArgumentException("Health cannot be less than 0 or more than maximal health.");
                    }
                    health = value;
                    ManageState();
                }              
            }
        }

        public double MaxHealth
        {
            get => maxHealth;            
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Maximal health cannot be less than 0.");
                }
                maxHealth = value;
            }
        }

        public StateHealth StateHealth { get => stateHealth; private set => stateHealth = value; }

        public int Experience
        {
            get => experience;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Experience cannot be less than 0.");
                }
                experience = value;
            }
        }

        public HashSet<State> States { get => states; }


        public bool AddState(State state)
        {
            if (Health != 0)
            {
                return states.Add(state);
            }
            return false;
        }
            
        public bool RemoveState(State state)
        {
            if (Health != 0)
            {
                return states.Remove(state);
            }
            return false;
        }

        public bool CanSpeakNow { get => canSpeakNow; set => canSpeakNow = value; }
        
        public bool CanMoveNow { get => canMoveNow; set => canMoveNow = value; }
       
        public int CompareTo(object obj)
        {
            if (obj is Character character)
            {
                return experience.CompareTo(character.experience);
            }
            throw new ArgumentException("obj is not type of Character.");
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
                canMoveNow = false;
                canSpeakNow = false;
            }
        }

        public void PickUpArtifact(Artifact artifact)
        {
            CheckIfDeadTryAct();
            inventory.Add(artifact);
        }

        /// <summary>
        /// Removes artifact from the inventory of a character.
        /// </summary>
        /// <param name="artifact"></param>
        /// <returns> True if artifact was successfully removed, otherwise returns False</returns>
        public bool RemoveArtifact(Artifact artifact) => inventory.Remove(artifact);
   

        /// <summary>
        /// Removes artifact to the inventory of other Character.
        /// </summary>
        /// <param name="another"></param>
        /// <param name="artifact"></param>
        /// <returns> True if succeed, otherwise False.</returns>
        public bool GiveArtifactToAnotherCharacter(Character another, Artifact artifact)
        {
            CheckIfDeadTryAct();
            bool wasInInventary = inventory.Remove(artifact);
            if (wasInInventary)
            {                
                 another.inventory.Add(artifact);
                 return true;
            }
            return false;
        }

        public bool StealArtifact(Character another, Artifact artifact)
        {
            CheckIfDeadTryAct();
            bool wasInInventary = another.inventory.Remove(artifact);
            if (wasInInventary)
            {
                inventory.Add(artifact);
                return true;
            }
            return false;
        }

        public int GetNumberOfArtifacts() => inventory.Count;

        public Artifact GetArtifactByIndex(int index) => inventory[index];
       
        public bool UseArtifact(Artifact artifact, Character another)
        {
            CheckIfDeadTryAct();
            int index = inventory.IndexOf(artifact);
            if (index != -1)
            {
                artifact.MagicEffect(this, another);
                return true;
            }
            return false;           
        }

        public bool UseArtifact(Artifact artifact)
        {
            CheckIfDeadTryAct();
            int index = inventory.IndexOf(artifact);
            if (index != -1)
            {
                (artifact as IMagic).MagicEffect(this);
                return true;
            }
            return false;
        }

        public bool UseArtifact(PoweredRenewableArtifact artifact, Character target, double power)
        {
            CheckIfDeadTryAct();
            int index = inventory.IndexOf(artifact);
            if (index != -1)
            {
                artifact.MagicEffect(this, target, power);
                return true;
            }
            return false;
                        
        }

        public bool UseArtifact(PoweredRenewableArtifact artifact, double power)
        {
            CheckIfDeadTryAct();
            int index = inventory.IndexOf(artifact);
            if (index != -1)
            {
                (artifact as IMagicPowered).MagicEffect(this, power);
                return true;
            }
            return false;
        }

        public void CheckIfDeadTryAct()
        {
            if (StateHealth == StateHealth.DEAD)
            {
                throw new DeadTryToactException("Dead character can't act.");
            }
        }

        public void Revive()
        {
            health = 1;
            ManageState();
        }

        public override string ToString()
        {
            return "ID: " + ID + "\n" + "Name: " + Name + "\n" + "Race: " + Race + "\n" +
            "Sex: " + Sex + "\n" + "Age: " + Age + "\n" + "Health: " + Health + "\n" +
            "State: " + StateHealth + "\n" + "Max health: " + MaxHealth + "\n" +
            "Ability to speak now: " + CanSpeakNow + "\n" +
            "Ability to move now: " + CanMoveNow + "\n" + "States: " + String.Join(", ", States);
        }
    }  
}
