using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Projectile : MonoBehaviour
{
    [SerializeField]
    private ProjectileContainer container = default;
    [SerializeField]
    private Weapon source = default;
    //[SerializeField]
    //private LayerMask collisionMask;
    private new SpriteRenderer renderer;
    private Rigidbody2D rb;
    //private CircleCollider2D circleCollider;
    //private BoxCollider2D boxCollider;

    public void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        renderer = GetComponent<SpriteRenderer>();
        //circleCollider = GetComponent<CircleCollider2D>();
        //boxCollider = GetComponent<BoxCollider2D>();
    }


    /// <summary>
    /// Applies the projectiles graphics/stats/mods, and begins its movement.
    /// </summary>
    public void Initialize (ProjectileStats _stats, ProjectileGraphics _graphics, ProjectileModifiers _mods, Weapon _source)
    {
        container.stats = _stats;
        container.graphics = _graphics;
        container.mods = _mods;
        source = _source;
        ApplyGraphics(_graphics);
        if (renderer!=null)
            renderer.enabled = true;
        StartMovement();
    }

    public void Initialize(ProjectileStats _stats, ProjectileGraphics _graphics, ProjectileModifiers _mods)
    {
        container.stats = _stats;
        container.graphics = _graphics;
        container.mods = _mods;
        source = null;
        ApplyGraphics(_graphics);
        if (renderer != null)
            renderer.enabled = true;
        StartMovement();
    }

    public void Initialize(ProjectileContainer _container, Weapon _source)
    {
        container.stats = _container.stats;
        container.graphics = _container.graphics;
        container.mods = _container.mods;
        source = _source;
        ApplyGraphics(_container.graphics);
        gameObject.SetActive(true);
        if (renderer != null)
            renderer.enabled = true;
        StartMovement();
    }

    public void Initialize(ProjectileContainer _container)
    {
        container.stats = _container.stats;
        container.graphics = _container.graphics;
        container.mods = _container.mods;
        source = null;
        ApplyGraphics(_container.graphics);
        gameObject.SetActive(true);
        if (renderer != null)
            renderer.enabled = true;
        StartMovement();
    }

    [Button]
    public void Initialize()
    {
        gameObject.SetActive(true);
        if (renderer != null)
            renderer.enabled = true;
        StartMovement();
    }

    public void StartMovement ()
    {
        rb.velocity = transform.right * (container.stats.speed + (container.stats.speedFlux*Random.Range(-1f,1f)));
        StartCoroutine(RangeTrigger((container.stats.range+Random.Range(-1f,+1f)*1f)/container.stats.speed));
    }

    private IEnumerator RangeTrigger (float delay)
    {
        yield return new WaitForSeconds(delay);
        OnRangeMet();
    }

    public void SetStats (ProjectileStats _stats)
    {
        container.stats = _stats;
    }

    public ProjectileStats GetStats()
    {
        return container.stats;
    }

    public void ApplyGraphics(ProjectileGraphics _graphics)
    {
        renderer.sprite = _graphics.sprite;
        renderer.color = _graphics.color;
        transform.localScale = Vector3.one * _graphics.size;
    }

    public ProjectileGraphics GetGraphics()
    {
        return container.graphics;
    }

    public void SetMods (ProjectileModifiers _mods)
    {
        container.mods = _mods;
    }

    public ProjectileModifiers GetMods()
    {
        return container.mods;
    }

    private void OnTriggerEnter2D (Collider2D col)
    {
        Entity other = col.GetComponentInParent<Entity>();
        if (other==null)
        {
            OnHitWall();
        }
        else if (Entity.CanAttack(this.container.stats.faction, other.faction))
        {
            OnHitTarget(other);
        }
    }

    public void OnHitTarget(Entity other)
    {
        foreach (ProjectileMod mod in container.mods.onHitTarget)
        {
            mod.OnHitTarget();
        }

        other.Damage(container.stats.damage, container.stats.element, out float damageDealt, out float damageResisted);

        OnRemove();
    }

    public void OnHitWall()
    {
        foreach (ProjectileMod mod in container.mods.onHitWall)
        {
            mod.OnHitWall();
        }
        OnRemove();
    }

    public void OnRangeMet ()
    {
        foreach (ProjectileMod mod in container.mods.onRangeMet)
        {
            mod.OnRangeMet();
        }
        OnRemove();
    }

    public void OnReflected ()
    {
        Debug.Log("OnReflected()");
        foreach (ProjectileMod mod in container.mods.onReflected)
        {
            mod.OnReflected();
        }
        OnRemove();
    }

    public void OnRemove ()
    {
        foreach (ProjectileMod mod in container.mods.onRemove)
        {
            mod.OnRemove();
        }
        gameObject.SetActive(false);
    }
}

[System.Serializable]
public class ProjectileContainer
{
    public ProjectileStats stats;
    public ProjectileGraphics graphics;
    public ProjectileModifiers mods;

    public ProjectileContainer ()
    {
        stats = new ProjectileStats();
        graphics = new ProjectileGraphics();
        mods = new ProjectileModifiers();
    }

    public ProjectileContainer (ProjectileStats _stats, ProjectileGraphics _graphics, ProjectileModifiers _mods)
    {
        stats = _stats;
        graphics = _graphics;
        mods = _mods;
    }
}

[System.Serializable]
public class ProjectileStats
{
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

    public ProjectileStats ()
    {
        faction = Faction.None;
        element = Element.None;
        damage = 0;
        speed = 0;
        speedFlux = 0;
        range = 100f;
    }

    public ProjectileStats(Faction _faction, float _damage, float _speed) // Simplified constructor
    {
        faction = _faction;
        element = Element.None;
        damage = _damage;
        speed = _speed;
        speedFlux = 0;
        range = 100;
    }

    public ProjectileStats(Faction _faction, Element _element, float _damage, float _speed, float _speedFlux, float _range)
    {
        faction = _faction;
        element = _element;
        damage = _damage;
        speed = _speed;
        speedFlux = _speedFlux;
        range = _range;
    }
}

[System.Serializable]
public class ProjectileGraphics
{
    public Sprite sprite;
    [Tooltip("Color filter for projectile")]
    public Color color;
    [Tooltip("Size of projectile, in meteres")]
    public float size;

    public ProjectileGraphics ()
    {
        sprite = null;
        color = Color.white;
        size = 1f;
    }

    public ProjectileGraphics (Sprite _sprite, Color _color, float _size)
    {
        sprite = _sprite;
        color = _color;
        size = _size;
    }
}

/*
[System.Serializable]
public class ProjectileModifiers
{
    public List<ProjectileMod> mods = new List<ProjectileMod>();
}
*/

[System.Serializable]
public class ProjectileModifiers
{
    public List<ProjectileMod> onHitTarget = new List<ProjectileMod>();
    public List<ProjectileMod> onHitWall = new List<ProjectileMod>();
    public List<ProjectileMod> onReflected = new List<ProjectileMod>();
    public List<ProjectileMod> onRangeMet = new List<ProjectileMod>();
    public List<ProjectileMod> onRemove = new List<ProjectileMod>();
}

[System.Serializable]
public class ProjectileMod
{
    public const int priority = 0;
    public string name = "Default";

    public ProjectileMod ()
    {
        name = "default";
    }

    public virtual void OnHitTarget () { }

    public virtual void OnHitWall () { }

    public virtual void OnReflected () { }

    public virtual void OnRangeMet () { }

    public virtual void OnRemove () { }
}

public class Mod_Dummy : ProjectileMod
{
    new public const int priority = 0;
    new public string name = "Dummy";

    public override void OnHitTarget(){}

    public override void OnHitWall(){}

    public override void OnReflected(){}

    public override void OnRangeMet(){}

    public override void OnRemove(){}
}