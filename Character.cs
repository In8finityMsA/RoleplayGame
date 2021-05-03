using System;
using System.Collections.Generic;
using System.Linq;
using KashTaskWPF.Artifacts;
using KashTaskWPF.Interface;
using KashTaskWPF.States;

namespace KashTaskWPF
{
    public enum StateHealth
    {
        NORMAL, WEAK, DEAD
    }

    public enum State
    {
        SICK, POISONED, PARALIZED, ARMOR
    }

    public enum Race
    {
        HUMAN, GNOME, ELF, ORC, GOBLIN
    }

    public enum Sex
    {
        MALE, FEMALE
    }

    public class DeadTryToActException: Exception
    {
        public DeadTryToActException(string message = "") : base(message) { }
    }
        
    public class Character: IComparable
    {
        private static int unique_ID = 0;

        private readonly int id;
        private string name;
        private Race race;
        private Sex sex;
              
        private int age;
        private double health;
        private StateHealth stateHealth;
        
        private Dictionary<State, AbstractState> statesDynamic = new Dictionary<State, AbstractState>();

        private double maxHealth;
        private int experience;
        private double hitPower;

        private bool canSpeakNow = true;
        private bool canMoveNow = true;

        private readonly List<Artifact> inventory = new List<Artifact>();
        
        public const int DEFAULT_AGE = 1;
        public const double DEFAULT_MAXHEALTH = 100;
        public const int DEFAULT_EXPERIENCE = 0;
        public const double DEFAULT_HITPOWER = 15;
        public Character(string name, Race race, Sex sex, int age = DEFAULT_AGE, double maxHealth = DEFAULT_MAXHEALTH, int experience = DEFAULT_EXPERIENCE, double hitPower = DEFAULT_HITPOWER)
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
            HitPower = hitPower;
        }

        public Character(Character other) : this(other.Name, other.Race, other.Sex, other.Age, other.MaxHealth, other.Experience)
        {
            health = other.Health;
            ManageState();
            
            inventory = other.inventory.Select( item => (Artifact)item.Clone() ).ToList(); //just copy
            HitPower = other.HitPower;
            canMoveNow = other.canMoveNow;
            canSpeakNow = other.canSpeakNow;
        }
        
        public int ID { get => id; }
        public string Name { get => name; set => name = value; }
        public Race Race { get => race; set => race = value; }
        public Sex Sex { get => sex; set => sex = value; }

        public List<Artifact> Inventory { get => inventory; }

        public int Age
        {
            get => age;
            set  
            {
                if (value <= 0)
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
                if (value >= health)
                {
                    if (value >= maxHealth)
                    {
                        health = maxHealth;
                    }
                    else
                    {
                        health = value;
                    }
                    ManageState();
                }
                else
                {
                    if (!statesDynamic.ContainsKey(State.ARMOR))
                    {
                        if (StateHealth != StateHealth.DEAD)
                        {
                            if (value < 0)
                            {
                                health = 0;
                            }
                            else
                            {
                                health = value;
                            }                
                            ManageState();
                        }
                    }
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

        public double HitPower
        {
            get => hitPower;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Hit power cannot be less than 0.");
                }
                hitPower = value;
            }
        }
        
        public Dictionary<State, AbstractState> StatesDynamic { get => statesDynamic; }

        public delegate void OnStepActions();
        public OnStepActions ActionsOnStep;

        public bool AddStateD(AbstractState state)
        {
            State stateType = state.State;
            try
            {
                AbstractState toChange = statesDynamic[stateType];
                if (toChange.Counter < state.Counter)
                {
                    statesDynamic[stateType] = state;

                    ActionsOnStep -= toChange.Step;//unsubscription
                    ActionsOnStep += state.Step;//subscription
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                statesDynamic[stateType] = state;


                //
                ActionsOnStep += state.Step; //subscription
                //
                return true;
            }
        }

        public bool RemoveStateD(State state)
        {
            try
            {
                AbstractState toRemove = statesDynamic[state];
                ActionsOnStep -= toRemove.Step;//unsubscription
                statesDynamic.Remove(state);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        

        public void EventHandler() //when step was made
        {
            if (ActionsOnStep != null)
            {
                ActionsOnStep();
            }          
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
            if (health <= 0)
            {
                stateHealth = StateHealth.DEAD;
                canMoveNow = false;
                canSpeakNow = false;
            }
            else if (health <= 0.1 * maxHealth)
            {
                stateHealth = StateHealth.WEAK;
            }
            else if (health >= 0.1 * maxHealth)
            {
                stateHealth = StateHealth.NORMAL;
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
                throw new DeadTryToActException("Dead character can't act.");
            }
        }

        public void Revive()
        {
            health = 1;
            ManageState();
        }

        public override string ToString()
        {
            return "Имя: " + Name + "\n" + "Раса: " + Race + "\n" + "Пол: " + Sex + "\n" +
            "Возраст: " + Age + "\n" + "Сила удара: " + HitPower + '\n' +
            "Здоровье: " + Health + "\n" + "Состояние здоровья: " + StateHealth + "\n" + "Максимальное здоровье: " + MaxHealth + "\n" +
            "Опыт: " + Experience + "\n" +
            "Возможность говорить сейчас: " + CanSpeakNow + "\n" +
            "Возможность двигаться сейчас: " + CanMoveNow + "\n" + "Состояния: " + String.Join(", ", statesDynamic.Keys);
        }

        public void Hit(Character target)
        {
            if (canMoveNow)
            {
                target.Health -= HitPower;
            }
        }
    }  
}
