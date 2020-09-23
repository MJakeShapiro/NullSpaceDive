using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(BigDick))]
[RequireComponent(typeof(SpriteRenderer))]
public class Projectile : MonoBehaviour
{
    #region Properties
    [SerializeField]
    public ProjectileContainer container = default;
    private Weapon source = default;
    private ProjLastEvent lastEvent = ProjLastEvent.Fired;
    private new SpriteRenderer renderer;

    private float scaleMult;
    private float rangeLeft; // Used to measure distance traveled

    private readonly float rayDuration = 3; // Applies to Debug visualizations
    private readonly bool useUpdate = true; // Whether or not to run the code in Update or FixedUpdate
    #endregion Properties

    #region Initializations
    private void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Applies the projectiles graphics/stats/mods, and begins its movement.
    /// </summary>
    [Button]
    public void Initialize()
    {
        lastEvent = ProjLastEvent.Fired;

        for (int i = 0; i < container.mods.Count; i++)
            container.mods[i].Initialize(this);

        ApplyGraphics(container.graphics);
        if (renderer != null)
            renderer.enabled = true;

        gameObject.SetActive(true);
        StartMovement();
    }

    /// <summary>
    /// Applies the projectiles graphics/stats/mods, and begins its movement.
    /// </summary>
    public void Initialize(ProjectileContainer _container, Weapon _source)
    {
        container = (ProjectileContainer)_container.Clone();
        source = _source;
        Initialize();
    }

    /// <summary>
    /// Applies the projectiles graphics
    /// </summary>
    public void ApplyGraphics(ProjectileGraphics _graphics)
    {
        renderer.sprite = _graphics.sprite;
        renderer.color = _graphics.color;
        transform.localScale = _graphics.GetScale();
        SetScale(1);
    }

    /// <summary>
    /// Starts moving the projectile and Activates is destroy-on-range trigger
    /// </summary>
    private void StartMovement ()
    {
        SetTimeBehind(0);
        velocity = transform.right * (container.stats.speed + (container.stats.speedFlux*Random.Range(-1f,1f)));
        SetMaxRange(container.stats.range);
    }

    /// <summary>
    /// Sets the total distance the projectile can travel before being removed
    /// </summary>
    public void SetMaxRange (float range)
    {
        if (range < 0)
            rangeLeft = -1;
        else
            rangeLeft = range + Random.Range(-1f, +1f);
    }
    #endregion Initializations

    #region Per-Frame
    private void Update ()
    {
        if (useUpdate)
            UpdateProj(Time.deltaTime);
    }

    private void FixedUpdate ()
    {
        if (!useUpdate)
            UpdateProj(Time.fixedDeltaTime);
    }

    private void UpdateProj (float delta)
    {
        OnUpdate(); // Triggers event for modifiers
        HandleMovement(delta); // Moves projectile and handles collisions
        //Disco(); // Removed due to time, budget, and practicality constraints
    }
    #endregion

    #region Physics

    #region Properties
    [SerializeField]
    private Vector2 velocity; // Current velocity
    private float colliderRadius; // Radius of collider for raycasts
    private float timeBehind; // Internal - tracks during physics calculations
    private RaycastHit2D lastHit; // last collision hit
    #endregion

    /// <summary>
    /// Moves the projectile according to their current velocity, checking for collisions along the way
    /// </summary>
    /// <param name="delta">Ammount of time to account for</param>
    private void HandleMovement (float delta)
    {
        float distance = velocity.magnitude * delta; // distance to travel this frame

        RaycastHit2D hit = PhysicsCast(velocity, distance); // projected path

        if (!OnCollision(hit, delta * (1 - hit.distance / distance))) // if collided with a valid object
            MoveProjectile(velocity * delta); // else move normally
    }

    /// <summary>
    /// Test the collision for validity, and move the projectile to the point of contact if so
    /// </summary>
    // <param name="movePosition">True to move position to point of collision</param>
    /// <returns>True if collided</returns>
    public bool OnCollision(RaycastHit2D hit, float timeBehind = -1)
    {
        #region Debug & Sanitization
        if (lastEvent == ProjLastEvent.Removed)
        {
            Debug.LogWarning("Uh oh, colliding after being removed!\n" + name);
            return false;
        }
        if (!hit)
            return false;
        #endregion

        Entity other = hit.collider.GetComponentInParent<Entity>();

        if (other == null || Entity.CanAttack(container.stats.faction, other.faction))
        {
            SetLastHit(hit);
            if (timeBehind > 0)
                SetTimeBehind(timeBehind);
            MoveProjectileTo(hit.centroid, true);

            if (other == null)
                OnHitWall();
            else
                OnHitTarget(other);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Moves the projectile by a Vector2 delta
    /// </summary>
    public void MoveProjectile (Vector2 delta, bool partial = false)
    {
        MoveProjectileTo(new Vector2(transform.position.x+delta.x, transform.position.y+delta.y), partial, delta.magnitude);
    }

    /// <summary>
    /// Moves the projectile to a new Vector2 position
    /// </summary>
    public void MoveProjectileTo (Vector2 newPosition, bool partial = false, float distance = -1)
    {
        if (distance < 0)
            distance = Vector2.Distance(transform.position, newPosition);
        if (rangeLeft > 0)
        {
            rangeLeft -= distance;
            if (rangeLeft <= 0) // Handle partial movement
            {
                transform.position += new Vector3(newPosition.x-transform.position.x, newPosition.y-transform.position.y, 0).normalized * -rangeLeft;
                OnRemove();
                return;
            }
        }

        #region Debug
#if UNITY_EDITOR
        GameManager.Utility.DrawBulletPath(newPosition, velocity, transform.position, GetColliderRadius(), partial);
#endif
        #endregion

        transform.position = newPosition;
    }

    /// <summary>
    /// Performs a phsyics cast, checking for collisions along the way
    /// </summary>
    /// <returns>The Raycasthit2D result of the cast</returns>
    public static RaycastHit2D PhysicsCast (Vector2 origin, Vector2 direction, float radius, float distance)
    {
        if (radius <= 0.05f) // Save resources by using a raycast approximation for small projectiles
            return Physics2D.Raycast(origin, direction, distance, GetProjectileMask());
        else
            return Physics2D.CircleCast(origin, radius, direction, distance, GetProjectileMask());
    }

    /// <summary>
    /// Performs a phsyics cast for the projectile, checking for collisions along the way
    /// </summary>
    /// <returns>The Raycasthit2D result of the cast</returns>
    public RaycastHit2D PhysicsCast(Vector2 direction, float distance)
    {
        return PhysicsCast(transform.position, direction, GetColliderRadius(), distance);
    }

    public static int PhysicsCastAll(Vector2 origin, Vector2 direction, float radius, float distance, RaycastHit2D[] results)
    {
        if (radius <= 0.05f) // Save resources by using a raycast approximation for small projectiles
            return Physics2D.RaycastNonAlloc(origin, direction, results, distance, GetProjectileMask());
        else
            return Physics2D.CircleCastNonAlloc(origin, radius, direction, results, distance, GetProjectileMask());
    }

    public int PhysicsCastAll (Vector2 direction, float distance, RaycastHit2D[] results)
    {
        return PhysicsCastAll (transform.position, direction, GetColliderRadius(), distance, results);
    }
    #endregion

    #region Physics Handlers
    /// <summary>
    /// LastHit tracks the last collision of the projectile
    /// </summary>
    public void SetLastHit (RaycastHit2D hit)
    {
        lastHit = hit;
    }

    /// <summary>
    /// LastHit tracks the last collision of the projectile
    /// </summary>
    public RaycastHit2D GetLastHit ()
    {
        return lastHit;
    }

    /// <summary>
    /// Sets the scale multiplier of the projectile
    /// </summary>
    public void SetScale(float _scaleMult)
    {
        scaleMult = (_scaleMult < 0 ? 0 : _scaleMult);
        colliderRadius = Mathf.Max(scaleMult * container.graphics.size / 2, 0.05f);
    }

    /// <summary>
    /// Returns the native collider radius of this projectile, in meters
    /// </summary>
    public float GetColliderRadius()
    {
        return colliderRadius;
    }

    /// <summary>
    /// Reflects the projectile towards the provided target
    /// </summary>
    public void ReflectTowards(Transform target)
    {
        SetDirection(target.position - transform.position);
        OnReflected(ReflectionType.None);
    }

    /// <summary>
    /// Sets the projectiles direction
    /// </summary>
    public void SetDirection(Vector3 dirr)
    {
        velocity = dirr.normalized * velocity.magnitude;
        transform.right = dirr;
    }
    #endregion

    #region General
    /// <summary>
    /// Cancels the range limitter for the projectile
    /// <para>Warning: Projectile will not self-destruct, may cause lag</para>
    /// </summary>
    public void CancelRangeTrigger ()
    {
        rangeLeft = -1;
    }

    /// <returns>This projectiles current velocity as a Vector2, in m/s</returns>
    public Vector2 GetVelocity ()
    {
        return velocity;
    }

    /// <returns>This projectiles lastEvent property</returns>
    public ProjLastEvent GetLastEvent ()
    {
        return lastEvent;
    }

    /// <summary>
    /// Used by internal physics calculations.
    /// <para>Represents the time remaining during the current frame</para>
    /// </summary>
    public void SetTimeBehind(float _timeBehind)
    {
        timeBehind = _timeBehind < 0 ? 0 : _timeBehind; // Sanitize Input, must be non-negative
    }

    /// <summary>
    /// Used by internal physics calculations.
    /// <para>Represents the time remaining during the current frame</para>
    /// </summary>
    public float GetTimeBehind()
    {
        return timeBehind;
    }
    #endregion General

    #region EventMethods
    private void OnUpdate()
    {
        ModReturn fate = ModReturn.Pass;
        for (int i=0; i<container.mods.Count; i++)
        {
            bool boo = container.mods[i].OnFixedUpdate(out ModReturn _fate);

            if (fate == ModReturn.Pass)
                fate = _fate;
            if (boo)
                break;
        }
        if (fate == ModReturn.Remove)
            OnRemove();
    }

    public void OnHitTarget(Entity other)
    {
        lastEvent = ProjLastEvent.HitTarget;
        ModReturn fate = ModReturn.Pass;
        bool cancelDamage = false;
        for (int i=0; i<container.mods.Count; i++)
        {
            bool boo = container.mods[i].OnHitTarget(out ModReturn _fate, out bool _cancelDamage);
            
            if (_cancelDamage)
                cancelDamage = true;
            if (fate == ModReturn.Pass)
                fate = _fate;
            if (boo)
                break;
        }
        if (!cancelDamage)
            other.Damage(container.stats.damage, container.stats.element, out float damageDealt, out float damageResisted);
        if (fate==ModReturn.Remove || fate==ModReturn.Pass)
            OnRemove();
    }

    public void OnHitWall()
    {
        lastEvent = ProjLastEvent.HitWall;
        ModReturn fate = ModReturn.Pass;
        for (int i=0; i<container.mods.Count; i++)
        {
            bool boo = container.mods[i].OnHitWall(out ModReturn _fate);

            if (fate == ModReturn.Pass)
                fate = _fate;
            if (boo)
                break;
        }
        if (fate==ModReturn.Remove || fate == ModReturn.Pass)
            OnRemove();
    }

    public void OnRangeMet ()
    {
        #region Debug
#if UNITY_EDITOR

        if (!gameObject.activeSelf)
        {
            Debug.Log("Projectile called OnRangeMet() for an inactive object *thonk face*");
            return;
        }
#endif
        #endregion

        lastEvent = ProjLastEvent.RangeMet;
        ModReturn fate = ModReturn.Pass;
        for (int i=0; i<container.mods.Count; i++)
        {
            bool boo = container.mods[i].OnRangeMet(out ModReturn _fate);

            if (fate == ModReturn.Pass)
                fate = _fate;
            if (boo)
                break;
        }
        if (fate==ModReturn.Remove || fate == ModReturn.Pass)
            OnRemove();
    }

    public void OnReflected(ReflectionType type)
    {
        Debug.Log("OnReflected()");
        lastEvent = ProjLastEvent.Reflected;
        for (int i=0; i<container.mods.Count; i++)
            container.mods[i].OnReflected();

        container.stats.faction = Entity.GetReflectedFaction(container.stats.faction);
        switch(type)
        {
            case ReflectionType.None:
                break;
            case ReflectionType.Random360:
                velocity = Random.insideUnitCircle * velocity.magnitude;
                break;
            case ReflectionType.ReverseDirection:
                velocity = -velocity;
                break;
            case ReflectionType.ReverseDirection90:
                velocity = Quaternion.Euler(0,0,Random.Range(-45,45)) * -velocity;
                break;
            case ReflectionType.TargetSource:
                velocity = (source.transform.position-transform.position).normalized * velocity.magnitude;
                break;
            case ReflectionType.TargetSource30:
                velocity = Quaternion.Euler(0,0,Random.Range(-15,15)) * (source.transform.position-transform.position).normalized * velocity.magnitude;
                break;
            case ReflectionType.TargetSource90:
                velocity = Quaternion.Euler(0,0,Random.Range(-45,45)) * (source.transform.position-transform.position).normalized * velocity.magnitude;
                break;
            default:
                #region DebugAlerts
                Debug.LogWarning("Uh oh! Looks wike someone made a Big Ol' fucky-wucky >w<\n Go ywell at StaikyWaiky to impwement these fweatures: " +type);
                #endregion
                break;
        }
    }

    /// <summary>
    /// Removes the projectile
    /// </summary>
    /// <param name="timeBehind">How long ago this should have happened, in seconds</param>
    public void OnRemove ()
    {
        #region Debug
#if UNITY_EDITOR
        if (GameManager.Utility._bulletDrawMode!=0)
        {
            GameManager.Utility.DrawX(transform.position, Color.red, radius: GetColliderRadius(), rayDuration);
            GameManager.Utility.DrawDirectionalCircle(transform.position, velocity, Color.red, radius: GetColliderRadius(), rayDuration, 31);
        }
#endif
        #endregion

        if (lastEvent != ProjLastEvent.RangeMet)
            CancelRangeTrigger();

        for (int i=0; i<container.mods.Count; i++)
            container.mods[i].OnRemove();

        lastEvent = ProjLastEvent.Removed;
        source.RemoveProjectile(gameObject);
        gameObject.SetActive(false);
    }
    #endregion CollisionMethods

    #region Utility
    /// <summary>
    /// Moves the projectile backwards to just before any collision
    /// <para>Note: Only call this function immediately after a collision</para>
    /// </summary>
    /// <returns>Percentage of expected distance traveled</returns>
    private float BackDatAssUp ()
    {
        Vector2 velocity = GetVelocity();
        float distanceTotal = velocity.magnitude * Time.fixedDeltaTime;
        Vector2 startPos = (Vector2)transform.position - (velocity * Time.fixedDeltaTime);
        float radius = container.graphics.size / 2;
        LayerMask mask = LayerMask.GetMask("Entity") | LayerMask.GetMask("Map");

        #region Debug
#if UNITY_EDITOR
        if (GameManager.Utility.drawDebug)
            GameManager.Utility.DrawCircle((Vector2)transform.position, new Color(1, 0.92f, 0.016f, .3f), radius, rayDuration, 31); // Original bullet end-position
#endif
        #endregion

        RaycastHit2D hit = Physics2D.CircleCast(startPos, radius, velocity, distanceTotal + 0.1f, mask);
        if (hit)
            transform.position = hit.centroid;
        return Mathf.Clamp01(((Vector2)transform.position-startPos).magnitude / distanceTotal);
    }

    /// <summary>
    /// Returns the layermask for projectile collisions
    /// </summary>
    public static LayerMask GetProjectileMask ()
    {
        return LayerMask.GetMask("Entity") | LayerMask.GetMask("Map");
    }
    #endregion

    #region Enums
    public enum ReflectionType
    {
        None,
        Random360,
        ReverseDirection,
        ReverseDirection90,
        TargetSource,
        TargetSource30,
        TargetSource90,
        UltraCoolMode
    }
    #endregion
}



[System.Serializable]
public class ProjectileContainer : System.ICloneable
{
    #region Properties
    public ProjectileStats stats;
    public ProjectileGraphics graphics;
    public List<ProjMod> mods;
    #endregion Properties

    #region Constructors
    public ProjectileContainer ()
    {
        stats = new ProjectileStats();
        graphics = new ProjectileGraphics();
        mods = new List<ProjMod>();
    }

    public ProjectileContainer (ProjectileStats _stats, ProjectileGraphics _graphics, List<ProjMod> _mods)
    {
        stats = _stats;
        graphics = _graphics;
        mods = ProjMod.CloneList(_mods);
    }

    protected ProjectileContainer (ProjectileContainer other)
    {
        stats = other.stats;
        graphics = other.graphics;
        mods = ProjMod.CloneList(other.mods);
    }

    public object Clone ()
    {
        return new ProjectileContainer(this);
    }
    #endregion Constructors

    #region ModifyModifiers
    /// <summary>
    /// Changes the priority of the supplied ProjMod Type, if one exists in mods
    /// </summary>
    /// <returns>True if sucessful</returns>
    public bool ChangeModPriority<TMod>(int newPriority)
    {
        foreach (ProjMod pm in mods)
            if (pm.GetType() == typeof(TMod))
            {
                pm.priority = newPriority;
                return true;
            }
        return false;
    }

    public void AddNewProjMod (ProjMod mod)
    {
        mods.Add((ProjMod)mod.Clone());
        mods[mods.Count-1].Initialize(null);
    }

    public void AddNewProjMod (ProjModType type)
    {
        AddNewProjMod(ProjMod.GetProjModFromType(type));
    }

    public bool RemoveProjMod(ProjMod mod)
    {
        foreach (ProjMod pm in mods)
            if (pm.GetType() == mod.GetType())
            {
                mods.Remove(pm);
                return true;
            }
        return false;
    }
    #endregion ModifyModifiers
}



[System.Serializable]
public class ProjectileStats
{
    #region Properties
    [Tooltip("Faction that opwns this projectile")]
    public Faction faction;
    [Tooltip("Element type of this projectile")]
    public Element element;
    [Tooltip("Raw damage of each projectile")]
    public float damage;
    [Tooltip("Average projectile speed in m/s")]
    public float speed;
    [Tooltip("+/- Speed fluctuation from average in m/s, also effects range")]
    public float speedFlux;
    [Tooltip("Max range before destroyed in m, affected by speedFlux")]
    public float range;
    #endregion Properties

    #region Constructors
    public ProjectileStats ()
    {
        faction = Faction.None;
        element = Element.None;
        damage = 0;
        speed = 0;
        speedFlux = 0;
        range = 100f;
    }

    public ProjectileStats(Faction _faction, Element _element, float _damage, float _speed, float _speedFlux = 0, float _range = 100)
    {
        faction = _faction;
        element = _element;
        damage = _damage;
        speed = _speed;
        speedFlux = _speedFlux;
        range = _range;
    }
    #endregion Constructors
}



[System.Serializable]
public class ProjectileGraphics
{
    #region Properties
    public Sprite sprite;
    [Tooltip("Color filter for projectile")]
    public Color color;
    [Min(0)][Tooltip("Size of projectile, in meteres")]
    public Vector2 scale;
    [Min(0)][Tooltip("Size of the circle collider, in meteres")]
    public float size; // Used to make colliders smaller than projectiles, or for non-circular projectiles
    #endregion Properties

    #region Constructors
    public ProjectileGraphics ()
    {
        sprite = null;
        color = Color.white;
        size = 1f;
        scale = Vector2.one;
    }

    public ProjectileGraphics (Sprite _sprite, Color _color, Vector2 _scale, float _size = 0.2f)
    {
        sprite = _sprite;
        color = _color;
        scale = _scale;
        size = _size;
    }

    public ProjectileGraphics (ProjectileGraphics pg)
    {
        sprite = pg.sprite; // shares reference as original
        color = pg.color;
        scale = pg.scale;
        size = pg.size;
    }
    #endregion Constructors

    #region General
    public Vector3 GetScale ()
    {
        return new Vector3(scale.x, scale.y, 1);
    }
    #endregion
}