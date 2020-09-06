using NaughtyAttributes;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public const float resistanceFactor = 0.25f; // Each level of resistance multiplies damage by this factor

    public Faction faction;
    public EntityReferenceContainer container;
    public EntityStats entityStats;
    //public EntityState state;
    //public bool activateOnStart;

    void Reset()
    {
        SetReferences();
    }

    protected virtual void Awake()
    {
        entityStats.Restore();
    }

    protected virtual void Start()
    {
        container.SetEntityReferences();
        //if (activateOnStart)
        //    state = EntityState.Active;
    }

    protected virtual void Update()
    {
        // Healing over time? idk
    }
    
    [Button]
    protected virtual void SetReferences()
    {
        if (container == null)
            container = new EntityReferenceContainer();

        if (container.controller == null)
            container.controller = GetComponentInChildren<EntityController>();
        if (container.movement == null)
            container.movement = GetComponentInChildren<EntityMovement>();
        if (container.aiming == null)
            container.aiming = GetComponentInChildren<EntityAiming>();
    }

    /// <summary>
    /// Damages the entity, based on the paramaters.
    /// </summary>
    /// <param name="damage">Incoming damage</param>
    /// <param name="element">Element type of incoming damage</param>
    /// <param name="damageDealt">Outgoing -> Damage actually dealt</param>
    /// <returns>True if the Entity was killed</returns>
    public bool Damage(float damage, Element element, out float damageDealt)
    {
        damageDealt = 0;
        if (damage <= 0) // Input sanitizing
        { Debug.Log($"Damages less than 1 not currently supported!\n{damage}, {element}"); return false; }

        bool killed = false;

        switch (element)
        {
            case Element.Plasma:
                float damageMult = 2f / (2f + (entityStats.GetShields()>0?1f:0f) + (entityStats.GetArmor()>0?1f:0f)); // return 1 for only hp, .667 for shields or armor, .5 for both
                if (entityStats.GetShields() > 0)
                {
                    killed = DamageShields(damage*damageMult, element, out float _damageDealt);
                    damageDealt += _damageDealt;
                }
                if (entityStats.GetArmor() > 0)
                {
                    killed = DamageArmor(damage*damageMult, element, out float _damageDealt);
                    damageDealt += _damageDealt;
                }
                if (entityStats.GetHealth() > 0)
                {
                    killed = DamageHealth(damage*damageMult, element, out float _damageDealt);
                    damageDealt += _damageDealt;
                }
                break;


            default:
                if (entityStats.GetShields() > 0)
                {
                    killed = DamageShields(damage, element, out float _damageDealt);
                    damageDealt += _damageDealt;
                    damage -= _damageDealt;
                }
                if (entityStats.GetArmor() > 0 && damage > 0)
                {
                    killed = DamageArmor(damage, element, out float _damageDealt);
                    damageDealt += _damageDealt;
                    damage -= _damageDealt;
                }
                if (entityStats.GetHealth() > 0 && damage > 0)
                {
                    killed = DamageHealth(damage, element, out float _damageDealt);
                    damageDealt += _damageDealt;
                    //damage -= _damageDealt; // Overkill damage, if ever needed
                }
                break;
        }
        return killed;
    }

    /// <summary>
    /// Damages shields directly, based on the paramaters.
    /// </summary>
    /// <param name="damage">Incoming damage</param>
    /// <param name="element">Element type of incoming damage</param>
    /// <param name="damageDealt">Outgoing -> Damage actually dealt</param>
    /// <returns>True if the Entity was killed</returns>
    public bool DamageShields(float damage, Element element, out float damageDealt)
    {
        damageDealt = 0;
        if (damage <= 0)
            return false;

        int resistanceLevel = Mathf.Clamp(entityStats.resistances.GetResistanceLevel(element) + (element==Element.Electric?1:0), -2, +2);
        float effectiveDamage = damage * (1+resistanceLevel*resistanceFactor);
        damageDealt = Mathf.Clamp(effectiveDamage, 0, entityStats.GetShields());
        entityStats.SetShields(entityStats.GetShields()-damageDealt);
        if (entityStats.GetShields() == 0)
        {
            OnShieldBreak ();
            if (entityStats.criticalType == EntityStats.HealthType.Shields)
            {
                OnDeath();
                return true;
            }
        }
#if UNITY_EDITOR
        else if (entityStats.GetShields() < 0)
            Debug.Log("Uh oh, shields set to less than 0 somehow!\n" + entityStats.GetShields());
#endif
        return false;
    }

    /// <summary>
    /// Damages armor directly, based on the paramaters.
    /// </summary>
    /// <param name="damage">Incoming damage</param>
    /// <param name="element">Element type of incoming damage</param>
    /// <param name="damageDealt">Outgoing -> Damage actually dealt</param>
    /// <returns>True if the Entity was killed</returns>
    public bool DamageArmor(float damage, Element element, out float damageDealt)
    {
        damageDealt = 0;
        if (damage <= 0)
            return false;

        int resistanceLevel = Mathf.Clamp(entityStats.resistances.GetResistanceLevel(element) + (element == Element.Acid ? 1 : 0), -2, +2);
        float effectiveDamage = damage * (1 + resistanceLevel * resistanceFactor);
        damageDealt = Mathf.Clamp(effectiveDamage, 0, entityStats.GetArmor());
        entityStats.SetArmor(entityStats.GetArmor()-damageDealt);
        if (entityStats.GetArmor() == 0)
        {
            OnArmorBreak ();
            if (entityStats.criticalType == EntityStats.HealthType.Armor)
            {
                OnDeath();
                return true;
            }
        }
#if UNITY_EDITOR
        else if (entityStats.GetArmor() < 0)
            Debug.Log("Uh oh, armor set to less than 0 somehow!\n" + entityStats.GetArmor());
#endif
        return false;
    }

    /// <summary>
    /// Damages health directly, based on the paramaters.
    /// </summary>
    /// <param name="damage">Incoming damage</param>
    /// <param name="element">Element type of incoming damage</param>
    /// <param name="damageDealt">Outgoing -> Damage actually dealt</param>
    /// <returns>True if the Entity was killed</returns>
    public bool DamageHealth(float damage, Element element, out float damageDealt)
    {
        damageDealt = 0;
        if (damage <= 0)
            return false;

        int resistanceLevel = Mathf.Clamp(entityStats.resistances.GetResistanceLevel(element) + (element == Element.Fire ? 1 : 0), -2, +2);
        float effectiveDamage = damage * (1 + resistanceLevel * resistanceFactor);
        damageDealt = Mathf.Clamp(effectiveDamage, 0, entityStats.GetHealth());
        entityStats.SetHealth(entityStats.GetHealth()-damageDealt);
        if (entityStats.GetHealth() == 0 && entityStats.criticalType == EntityStats.HealthType.Health)
        {
            OnDeath();
            return true;
        }
#if UNITY_EDITOR
        else if (entityStats.GetHealth() < 0)
            Debug.Log("Uh oh, health set to less than 0 somehow!\n" + entityStats.GetHealth());
#endif
        return false;
    }

    /// <summary>
    /// Is called whenever the Entitys shield is set to 0
    /// </summary>
    protected void OnShieldBreak ()
    {
        container.controller?.OnShieldBreak();
    }

    /// <summary>
    /// Is called whenever the Entitys armor is set to 0
    /// </summary>
    protected void OnArmorBreak ()
    {
        container.controller?.OnArmorBreak();
    }

    /// <summary>
    /// Is called whenever the Entitys health is set to 0
    /// </summary>
    protected void OnHealthBreak ()
    {
        container.controller?.OnHealthBreak();
    }

    /// <summary>
    /// Is called whenever the Entity should be killed
    /// </summary>
    protected void OnDeath ()
    {
        if (container.controller != null && container.controller.OnDeath())
            return;
        else
            DefaultDeathSequence();
    }

    protected void DefaultDeathSequence()
    {
        //TEMPORARY
        Debug.Log("Killing Entity: " + name);
        Destroy(gameObject);
    }

    /// <summary>
    /// Tests whether two factions can attack each other
    /// </summary>
    /// <param name="fac1">Attacking Faction</param>
    /// <param name="fac2">Deffending Faction</param>
    /// <returns>True if they can damage each other</returns>
    public static bool CanAttack(Faction fac1, Faction fac2)
    {
        if ((fac1 == Faction.Player && fac2 == Faction.Ally) || (fac1 == Faction.Ally && fac2 == Faction.Player))
            return false;
        else
            return (fac1 != fac2);
    }

    [System.Serializable]
    public class EntityReferenceContainer
    {
        public EntityController controller;
        public EntityMovement movement;
        public EntityAiming aiming;

        public void SetEntityReferences ()
        {
            controller?.SetEntityReference(this);
            movement?.SetEntityReference(this);
            aiming?.SetEntityReference(this);
        }
    }
}

[System.Serializable]
public class EntityStats
{
    [ReadOnly][SerializeField]
    protected float health;
    [ProgressBar("Health", 100, EColor.Red)][SerializeField]
    private float healthDisplay = 0;

    [ReadOnly][SerializeField]
    protected float armor;
    [ProgressBar("Armor", 100, EColor.Orange)][SerializeField]
    private float armorDisplay = 0;

    [ReadOnly][SerializeField]
    protected float shields;
    [ProgressBar("Shields", 100, EColor.Blue)][SerializeField]
    private float shieldsDisplay = 0;

    [Min(0)]
    public int maxHealth = 100;
    [Min(0)]
    public int maxArmor = 0;
    [Min(0)]
    public int maxShields = 0;

    public HealthType criticalType; // Which value kills on 0

    public Resistances resistances;

    /// <summary>
    /// Restores values to full
    /// </summary>
    public void Restore ()
    {
        SetHealth(maxHealth);
        SetArmor(maxArmor);
        SetShields(maxShields);
    }

    /// <summary>
    /// Public accessor, updates inspector views as well
    /// </summary>
    /// <param name="_health">Value to set health to</param>
    public void SetHealth(float _health)
    {
        health = Mathf.Clamp(_health, 0, maxHealth);
        if (maxHealth<=0)
            healthDisplay = 0;
        else
            healthDisplay = 100*health / maxHealth;
    }

    public void SetArmor(float _armor)
    {
        armor = Mathf.Clamp(_armor, 0, maxArmor);
        if (maxArmor <= 0)
            armorDisplay = 0;
        else
            armorDisplay = 100 * armor / maxArmor;
    }

    public void SetShields(float _shields)
    {
        shields = Mathf.Clamp(_shields, 0, maxShields);
        if (maxShields <= 0)
            shieldsDisplay = 0;
        else
            shieldsDisplay = 100 * shields / maxShields;
    }

    /// <summary>
    /// Public accessor for Entity health
    /// </summary>
    /// <returns>Entity health</returns>
    public float GetHealth ()
    {
        return health;
    }

    public float GetArmor ()
    {
        return armor;
    }

    public float GetShields ()
    {
        return shields;
    }

    public enum HealthType
    {
        Health,
        Armor,
        Shields
    }

    public enum ResistanceLevel
    {
        None = 0,
        VeryWeak = 2,
        Weak = 1,
        Resistant = -1,
        VeryResistant = -2
    }

    [System.Serializable]
    public class Resistances
    {
        public ResistanceLevel basic;
        public ResistanceLevel acid;
        public ResistanceLevel electric;
        public ResistanceLevel explosive;
        public ResistanceLevel fire;
        public ResistanceLevel frost;
        public ResistanceLevel plasma;
        public ResistanceLevel radiation;
        public ResistanceLevel water;
        public ResistanceLevel wind;


        /// <param name="element">Type to test resistance for</param>
        /// <returns>This entitys resistance level for the supplied element</returns>
        public int GetResistanceLevel (Element element)
        {
            switch (element)
            {
                case Element.None:
                    return (int)basic;
                case Element.Acid:
                    return (int)acid;
                case Element.Electric:
                    return (int)electric;
                case Element.Explosive:
                    return (int)explosive;
                case Element.Fire:
                    return (int)fire;
                case Element.Frost:
                    return (int)frost;
                case Element.Plasma:
                    return (int)plasma;
                case Element.Radiation:
                    return (int)radiation;
                case Element.Void:
                    return 4;
                case Element.Water:
                    return (int)water;
                case Element.Wind:
                    return (int)wind;
                default:
                    Debug.LogWarning($"Unsupported element '{element}' in Entity.cs -> Resitances");
                    return 0;
            }
        }
    }
}

public enum Faction
{
    None = 0, // Represents obstacles
    Player, // Damages enemy/environment, triggers player effects
    Ally, // Damages enemy/environment
    Enemy, // Damages player/environment
    ReflectedEnemy, // Damages player/enemy/environment, triggers player effects
    Hazard, // Damages player/enemy/environment
    ReflectedHazard // Damages player/enemy/environment, triggers player effects
}

public enum Element
{
    None = 0,
    Acid,
    Electric,
    Explosive,
    Fire,
    Frost,
    Plasma,
    Radiation,
    Void,
    Water,
    Wind
}