using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

[System.Serializable]
public class ProjMod : System.ICloneable
{
    #region Properties
    public ProjModType type = ProjModType.None;
    [Tooltip("Execution order of modifiers")]
    public int priority = 0;
    protected Projectile proj;
    #endregion

    #region Constructors
    public ProjMod ()
    {
        type = ProjModType.None;
        priority = 0;
    }

    public ProjMod(ProjMod pm)
    {
        type = pm.type;
        priority = pm.priority;
        proj = pm.proj;
    }

    public virtual object Clone ()
    {
        return new ProjMod(this);
    }
    #endregion

    #region Initialization
    public virtual void Initialize(Projectile _parent)
    {
        proj = _parent;
    }
    #endregion

    #region EventMethods

    /// <summary>
    /// Called every frame
    /// </summary>
    /// <returns>True to break mod loop</returns>
    public virtual bool OnFixedUpdate (out ModReturn modReturn)
    {
        modReturn = ModReturn.Pass;
        return false;
    }

    /// <summary>
    /// Called whenever the projectile hits a valid target
    /// </summary>
    /// <returns>True to break mod loop</returns>
    public virtual bool OnHitTarget (out ModReturn modReturn, out bool cancelDamage)
    {
        modReturn = ModReturn.Pass;
        cancelDamage = false;
        return false;
    }

    /// <summary>
    /// Called when the projectile collides with a wall
    /// </summary>
    /// <returns>True to break mod loop</returns>
    public virtual bool OnHitWall (out ModReturn modReturn)
    {
        modReturn = ModReturn.Pass;
        return false;
    }

    /// <summary>
    /// Called when the projectile is reflected
    /// </summary>
    public virtual void OnReflected () { }

    /// <summary>
    /// Called when the projectile reaches its max range
    /// </summary>
    /// <returns>True to break mod loop</returns>
    public virtual bool OnRangeMet (out ModReturn modReturn)
    {
        modReturn = ModReturn.Pass;
        return false;
    }

    /// <summary>
    /// Called when the projectile reaches the end of its life - for any reason
    /// </summary>
    public virtual void OnRemove () { }
    #endregion

    #region General
    protected void DestroyBullet ()
    {
        proj.OnRemove();
    }

    /// <summary>
    /// Removes the ProjMod from the projectile, effectively destroying it
    /// </summary>
    public void RemoveThisMod ()
    {
        proj.container.RemoveProjMod(this);
    }
    #endregion

    #region StaticMethods
    /// <returns>A new ProjMod child instance based on the supplied ProjModType</returns>
    static public ProjMod GetProjModFromType(ProjModType type)
    {
        switch (type)
        {
            case ProjModType.None:
                return new ProjMod();
            case ProjModType.Bouncy:
                return new ProjMod_Bouncy();
            case ProjModType.Dummy:
                return new ProjMod_Dummy();
            case ProjModType.Explosive:
                return new ProjMod_Explosive();
            case ProjModType.Helix:
                return new ProjMod_Helix();
            case ProjModType.Pierce:
                return new ProjMod_Pierce();
            default:
                //Debug.LogWarning($"Unsuported ProjModType: {type}");
                return null;
        }
    }

    static public List<ProjMod> CloneList (List<ProjMod> other)
    {
        List<ProjMod> newMods = new List<ProjMod>();
        for (int i = 0; i < other.Count; i++)
            newMods.Add((ProjMod)other[i].Clone());
        return newMods;
    }

    static public bool TestModTypes(out int infractions, out int totalCases)
    {
        infractions = 0;
        totalCases = System.Enum.GetNames(typeof(ProjModType)).Length;

        for (int i = 1; i < totalCases; i++)
        {
            ProjMod projMod = GetProjModFromType((ProjModType)i);
            if (projMod == null)
                Debug.LogWarning($"Infraction {++infractions}:\nUnsupported case '{(ProjModType)i}'");
            else if (projMod.Clone().GetType() != projMod.GetType())
                Debug.LogWarning($"Infraction {++infractions}:\nMissing Clone constructor for '{(ProjModType)i}'");
            //else if ("Your mom is gay"!="true")
                //Debug.LogComeback("No u");
        }

        if (infractions > 0)
            return false;
        return true;
    }
    #endregion
}



[System.Serializable]
public class ProjMod_Dummy : ProjMod
{
    #region Properties
    public ModReturn targetFate = ModReturn.Pass;
    public ModReturn wallFate = ModReturn.Pass;
    public ModReturn rangeFate = ModReturn.Pass;
    #endregion

    #region Constructors
    public ProjMod_Dummy ()
    {
        type = ProjModType.Dummy;
    }

    public ProjMod_Dummy(ProjMod_Dummy other)
    {
        type = other.type;
        priority = other.priority;
        targetFate = other.targetFate;
        wallFate = other.wallFate;
        rangeFate = other.rangeFate;
    }

    public override object Clone ()
    {
        return new ProjMod_Dummy(this);
    }
    #endregion

    #region Methods
    public override bool OnHitTarget(out ModReturn modReturn, out bool cancelDamage)
    {
        modReturn = targetFate;
        cancelDamage = false;
        return true;
    }

    public override bool OnHitWall(out ModReturn modReturn)
    {
        modReturn = wallFate;
        return false;
    }

    public override bool OnRangeMet(out ModReturn modReturn)
    {
        modReturn = rangeFate;
        return false;
    }
    #endregion
}


[System.Serializable]
public class ProjMod_Bouncy : ProjMod_Dummy
{
    #region Properties
    public int bouncesRemaining = 2;
    public bool bounceOffTarget = false;
    public bool bounceOffWall = true;
    #endregion

    #region Constructors
    public ProjMod_Bouncy()
    {
        type = ProjModType.Bouncy;
    }

    public ProjMod_Bouncy(ProjMod_Bouncy source)
    {
        type = source.type;
        priority = source.priority;
        bouncesRemaining = source.bouncesRemaining;
        bounceOffTarget = source.bounceOffTarget;
        bounceOffWall = source.bounceOffWall;
    }

    public override object Clone()
    {
        return new ProjMod_Bouncy(this);
    }
    #endregion

    #region Initialization
    public override void Initialize(Projectile _proj)
    {
        base.Initialize(_proj);
        priority = bouncesRemaining;
    }
    #endregion

    #region EventMethods
    public override bool OnHitTarget(out ModReturn modReturn, out bool cancelDamage)
    {
        cancelDamage = false;
        if (bounceOffTarget)
        {
            Bounce();
            modReturn = ModReturn.Keep;
            return true;
        }
        modReturn = ModReturn.Pass;
        return false;
    }

    public override bool OnHitWall(out ModReturn modReturn)
    {
        if (bounceOffWall)
        {
            Bounce();
            modReturn = ModReturn.Keep;
            return true;
        }
        modReturn = ModReturn.Pass;
        return false;
    }
    #endregion

    #region General
    protected void Bounce ()
    {
        #region Properties
        float rayDuration = 2;
        Vector2 velocity = proj.GetVelocity();
        RaycastHit2D lastHit = proj.GetLastHit();
        float timeBehind = proj.GetTimeBehind();
        float totalDistance = velocity.magnitude * timeBehind;
        #endregion

        #region Debug
        if (!lastHit)
        {
            Debug.LogWarning("Couldn't find the referenced collision for the projectile!\nHow'd we even get here?");
            return;
        }
#if UNITY_EDITOR
        if (GameManager.Utility.drawDebug)
            Debug.DrawRay(lastHit.point, lastHit.normal * 0.1f, Color.white, rayDuration); // hit normal
#endif
        #endregion Debug

        Vector2 newDir = Vector2.Reflect(velocity, lastHit.normal);
        proj.SetDirection(newDir);
        ReduceBounceCount();

        RaycastHit2D hit = proj.PhysicsCast(newDir, totalDistance);

        if (!proj.OnCollision(hit, timeBehind * (1 - hit.distance / totalDistance)))
            proj.MoveProjectile(newDir.normalized * totalDistance, false);
    }

public int ReduceBounceCount ()
    {
        priority = --bouncesRemaining; // temp display for inspector
        if (bouncesRemaining<=0)
            RemoveThisMod();
        return bouncesRemaining;
    }
    #endregion
}


public class ProjMod_Explosive : ProjMod_Dummy
{
    #region Properties
    public bool explodeOnTarget = true;
    public bool explodeOnWall = false;
    public bool explodeOnRangeMet = true;
    #endregion

    #region Constructors
    public ProjMod_Explosive()
    {
        type = ProjModType.Explosive;
        targetFate = ModReturn.Remove;
    }

    public ProjMod_Explosive(ProjMod_Explosive source)
    {
        type = source.type;
        priority = source.priority;
        targetFate = source.targetFate;
        wallFate = source.wallFate;
        rangeFate = source.rangeFate;
        explodeOnTarget = source.explodeOnTarget;
        explodeOnWall = source.explodeOnWall;
        explodeOnRangeMet = source.explodeOnRangeMet;
    }

    public override object Clone()
    {
        return new ProjMod_Explosive(this);
    }
    #endregion

    #region EventMethods
    public override bool OnHitTarget (out ModReturn modReturn, out bool cancelDamage)
    {
        modReturn = targetFate;
        cancelDamage = false;
        return true;
    }

    public override bool OnHitWall (out ModReturn modReturn)
    {
        modReturn = wallFate;
        return false;
    }

    public override bool OnRangeMet (out ModReturn modReturn)
    {
        modReturn = rangeFate;
        return false;
    }

    public override void OnRemove()
    {
        switch (proj.GetLastEvent())
        {
            case ProjLastEvent.HitTarget:
                if (explodeOnTarget)
                    Explode();
                break;
            case ProjLastEvent.HitWall:
                if (explodeOnWall)
                    Explode();
                break;
            case ProjLastEvent.RangeMet:
                if (explodeOnRangeMet)
                    Explode();
                break;
            default:
                break;
        }
    }
    #endregion

    #region Methods
    protected void Explode ()
    {
        #region Debug
#if UNITY_EDITOR

        Debug.Log("Boom, baby!\n" + proj.transform.position);
#endif
        #endregion
    }
    #endregion
}


public class ProjMod_Helix : ProjMod_Dummy
{
    #region Properties
    public float width = 1;
    #endregion

    #region Constructors
    public ProjMod_Helix ()
    {
        type = ProjModType.Helix;
    }

    public ProjMod_Helix (ProjMod_Helix source)
    {
        type = source.type;
        priority = source.priority;
        width = source.width;
    }

    public override object Clone ()
    {
        return new ProjMod_Helix(this);
    }
    #endregion

    #region EventMethods
    public override bool OnFixedUpdate (out ModReturn modReturn)
    {
        modReturn = ModReturn.Pass;
        return false;
    }
    #endregion
}


public class ProjMod_Pierce : ProjMod
{
    #region Properties
    public int piercesRemaining = 1;
    public bool pierceTarget = true;
    public bool pierceWall = false;
    #endregion

    #region Constructors
    public ProjMod_Pierce()
    {
        type = ProjModType.Pierce;
    }

    public ProjMod_Pierce(ProjMod_Pierce source)
    {
        type = source.type;
        priority = source.priority;
        piercesRemaining = source.piercesRemaining;
        pierceTarget = source.pierceTarget;
        pierceWall = source.pierceWall;
    }

    public override object Clone()
    {
        return new ProjMod_Pierce(this);
    }
    #endregion

    #region EventMethods
    public override bool OnHitTarget(out ModReturn modReturn, out bool cancelDamage)
    {
        cancelDamage = false;
        if (pierceTarget)
        {
            modReturn = ModReturn.Keep;
            Pierce();
        }
        else
            modReturn = ModReturn.Pass;
        return false;
    }

    public override bool OnHitWall(out ModReturn modReturn)
    {
        if (pierceWall)
        {
            modReturn = ModReturn.Keep;
            Pierce();
        }
        else
            modReturn = ModReturn.Pass;
        return false;
    }
    #endregion

    #region Methods
    protected void Pierce()
    {
        piercesRemaining--;

        #region Debug
#if UNITY_EDITOR
        Debug.Log("Piercing!\nRemaining:" + piercesRemaining);
#endif
        #endregion

        if (piercesRemaining<=0)
        {
            RemoveThisMod();
        }
    }
    #endregion
}

#region Enums
public enum ModReturn
{
    Remove = -1,
    Pass = 0,
    Keep = 1
}

public enum ProjLastEvent
{
    Fired = 0,
    HitTarget,
    HitWall,
    Reflected,
    RangeMet,
    Removed,
    ForceDestroyed
}

public enum ProjModType
{
    None = -0, // Breaks if you remove the negative sign, no touch
    Bouncy, // Like your mother
    Dummy, // Does nothing, in case we ever need that? "yolo"
    Explosive, // Like me with your mother
    Helix, // Wavy!
    Pierce // Two birds at once
}
#endregion

#region Old Bouncy
/*
public class ProjMod_Bouncy : ProjMod_Dummy
{
    #region Properties
    public int bouncesRemaining = 2;
    public bool bounceOffTarget = true;
    public bool bounceOffWall = true;

    protected int lastFrameBounced = -1;
    protected float distanceTraveledThisFrame = 0;
    protected float bounceMod = 1;
    #endregion

    #region Constructors
    public ProjMod_Bouncy()
    {
        type = ProjModType.Bouncy;
    }

    public ProjMod_Bouncy(ProjMod_Bouncy source)
    {
        type = source.type;
        priority = source.priority;
        bouncesRemaining = source.bouncesRemaining;
        bounceOffTarget = source.bounceOffTarget;
        bounceOffWall = source.bounceOffWall;
        lastFrameBounced = source.lastFrameBounced;
        distanceTraveledThisFrame = source.distanceTraveledThisFrame;
    }

    public override object Clone()
    {
        return new ProjMod_Bouncy(this);
    }
    #endregion

    #region Initialization
    public override void Initialize(Projectile _proj)
    {
        base.Initialize(_proj);
        priority = bouncesRemaining;
    }
    #endregion

    #region EventMethods
    public override bool OnHitTarget(out ModReturn modReturn, out bool cancelDamage)
    {
        cancelDamage = false;
        if (bounceOffTarget)
        {
            Bounce();
            modReturn = ModReturn.Keep;
            return true;
        }
        modReturn = ModReturn.Pass;
        return false;
    }

    public override bool OnHitWall(out ModReturn modReturn)
    {
        if (bounceOffWall)
        {
            Bounce();
            modReturn = ModReturn.Keep;
            return true;
        }
        modReturn = ModReturn.Pass;
        return false;
    }
    #endregion

    #region General
    protected void Bounce ()
    {
        float rayDuration = 2;
        if (lastFrameBounced != Time.frameCount)
        {
            lastFrameBounced = Time.frameCount;
            distanceTraveledThisFrame = 0;
        }
        Vector2 velocity = proj.GetVelocity() * bounceMod;
        float distanceTotal = bounceMod * velocity.magnitude*Time.fixedDeltaTime - distanceTraveledThisFrame;
        Vector2 startPos = (Vector2)proj.transform.position - (velocity*Time.fixedDeltaTime);
        float radius = proj.GetColliderRadius();

        if (GameManager.Utility.drawDebug)
        {
            //Debug.DrawRay(startPos, velocity.normalized * distanceTotal, Color.yellow, rayDuration); // path originally taken by proj
            GameManager.Utility.DrawArrow(startPos, velocity.normalized * distanceTotal, Color.yellow, rayDuration);
            GameManager.Utility.DrawCircle((Vector2)proj.transform.position, new Color (1,0.92f,0.016f,.4f), radius, rayDuration, 31);
        }
        RaycastHit2D hit = Physics2D.CircleCast(startPos, radius, velocity, distanceTotal + 0.1f, Projectile.GetProjectileMask());
        if (hit)
        {
            float distTraveled = (startPos - hit.centroid).magnitude;
            if (distTraveled > distanceTotal)
                distanceTotal = distTraveled;

            distanceTraveledThisFrame += distTraveled;
            Vector2 newDir = Vector2.Reflect(velocity, hit.normal);
            Vector3 newPos = hit.centroid + (newDir.normalized * (distanceTotal - distTraveled));            

            proj.transform.position = newPos;
            proj.SetDirection(newDir);
            ReduceBounceCount();

            GameManager.Utility.DrawBulletPath(hit.centroid, newDir, radius, true);
            if (GameManager.Utility.drawDebug)
            {
                Debug.DrawLine(startPos, hit.centroid, Color.blue, rayDuration); // modified path taken, up to wall
                Debug.DrawRay(hit.point, hit.normal * 0.1f, Color.white, rayDuration); // hit normal
                Debug.DrawRay(hit.centroid, newDir.normalized * Mathf.Min((distanceTotal-distTraveled),0), Color.green, rayDuration); // new projected path after wall
            }

            #region SecondBounce
            if (true)
            {
                distanceTotal -= distTraveled;
                if (distanceTotal < 0)
                {
                    Debug.Log(distanceTotal);
                    distanceTotal = 0;
                }
                startPos = hit.centroid;

                hit = Physics2D.CircleCast(startPos, radius, newDir, distanceTotal, Projectile.GetProjectileMask()); // Test if colliding again
                if (hit)
                {
                    if (GameManager.Utility.drawDebug)
                        Debug.DrawLine(startPos, hit.centroid, Color.cyan, rayDuration);
                    Debug.Log("Double Bounce, Radical!\nFrame: "+lastFrameBounced);
                    proj.OnCollision(hit.collider);
                }
            }
            #endregion SecondBounce
        }
    }

    public int ReduceBounceCount ()
    {
        priority = --bouncesRemaining; // temp
        if (bouncesRemaining<=0)
            RemoveThisMod();
        return bouncesRemaining;
    }
    #endregion
}
*/
#endregion