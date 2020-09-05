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
    private CircleCollider2D circleCollider;
    private BoxCollider2D boxCollider;

    public void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        renderer = GetComponent<SpriteRenderer>();
        circleCollider = GetComponent<CircleCollider2D>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

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
        rb.velocity = transform.right * container.stats.speed;
        StartCoroutine(RangeTrigger(container.stats.range/ container.stats.speed));
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
        //Debug.Log("OnTriggerEnter2D()");
        /*
        Entity other = col.GetComponent<Entity>();
        if (other==null)
        {
            OnHitWall();
        }
        else if (FactionManager.CanAttack(this.container.stats.faction, other.faction))
        {
            OnHitTarget();
        }
        */
        OnHitTarget();
    }

    public void OnHitTarget()
    {
        //Debug.Log("OnHitTarget()");
        foreach (ProjectileMod mod in container.mods.onHitTarget)
        {
            mod.OnHitTarget();
        }
        OnRemove();
    }

    public void OnHitWall()
    {
        //Debug.Log("OnHitWall()");
        foreach (ProjectileMod mod in container.mods.onHitWall)
        {
            mod.OnHitWall();
        }
        OnRemove();
    }

    public void OnRangeMet ()
    {
        //Debug.Log("OnRangeMet()");
        foreach (ProjectileMod mod in container.mods.onRangeMet)
        {
            mod.OnRangeMet();
        }
        OnRemove();
    }

    public void OnReflected ()
    {
        //Debug.Log("OnReflected()");
        foreach (ProjectileMod mod in container.mods.onReflected)
        {
            mod.OnReflected();
        }
        OnRemove();
    }

    public void OnRemove ()
    {
        //Debug.Log("OnRemove()");
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
    [HideInInspector]
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
    public Faction faction;
    public Element element;
    public float damage;
    public float speed;
    public float range;

    public ProjectileStats ()
    {
        faction = Faction.None;
        element = Element.None;
        damage = 0;
        speed = 0;
        range = 100f;
    }

    public ProjectileStats(Faction _faction, Element _element, float _damage, float _speed, float _range)
    {
        faction = _faction;
        element = _element;
        damage = _damage;
        speed = _speed;
        range = _range;
    }
}

[System.Serializable]
public class ProjectileGraphics
{
    public Sprite sprite;
    public Color color;
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
        //name = "default";
    }

    public virtual void OnHitTarget ()
    {

    }

    public virtual void OnHitWall ()
    {

    }

    public virtual void OnReflected ()
    {

    }

    public virtual void OnRangeMet ()
    {

    }

    public virtual void OnRemove ()
    {

    }
}

public class Mod_Dummy : ProjectileMod
{
    new public const int priority = 0;
    new public string name = "Dummy";

    public override void OnHitTarget()
    {

    }

    public override void OnHitWall()
    {

    }

    public override void OnReflected()
    {

    }

    public override void OnRangeMet()
    {

    }

    public override void OnRemove()
    {

    }
}