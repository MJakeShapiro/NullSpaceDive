using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Weapon
{
    #region Properties
    public FiringType firingType = FiringType.Automatic;
    public AmmoType ammoType = AmmoType.Bullet;
    public ReloadType reloadType = ReloadType.Clip;
    public GunStats stats;
    //[SerializeField]
    public Magazine mag;

    public GunState state = GunState.Idle;

    [Tooltip("Transform to fire bullet from")]
    public Transform barrelTip;

    public bool action2IsReload = true;
    public bool reloadOnEmptyFire = true;

    protected List<GameObject> activeBullets = new List<GameObject>();

    [SerializeField]
    protected int burstIndex = -1; // Counts down. -1 is off.
    protected float timeEquipped = -1;
    protected float reloadStarted = -1;
    [SerializeField]
    protected float lastBurstStarted = -1;
    [SerializeField]
    protected float lastFired = -1;

    protected bool action1Released = true;
    protected bool action2Released = true;
    #endregion Properties

    #region Initialization
    protected void Awake ()
    {
        //if (weaponType == WeaponType.Null)
        //    Debug.LogWarning("Null WeaponType set for: " + gameObject.name);
    }

    public override bool TestValidity()
    {
        int infractions = 0;

        if (weaponType == WeaponType.Null)
        {
            Debug.LogWarning("Null WeaponType set for: " + gameObject.name+"\nPlease set proper value in prefab");
            infractions++;
        }

        if (stats.burstCount == 1)
        {
            stats.burstCount = 0;
            Debug.LogWarning("BurstCount improperly set for: "+weaponType.ToString()+"\nPlase use 0 instead of 1");
            infractions++;
        }

        if (stats.burstCount > 1 && stats.burstDelay < stats.burstCount * stats.fireDelay)
        {
            stats.burstDelay = stats.burstCount * stats.fireDelay;
            Debug.LogWarning("Burst delay too short on "+weaponType.ToString()+"\nSetting to: "+stats.burstDelay);
            infractions++;
        }

        if (mag==null)
        {
            Debug.LogWarning("Mag reference not set on " + weaponType.ToString() + "\nPlease set proper value in prefab");
            infractions++;
        }

        if (barrelTip==null)
        {
            Debug.LogWarning("BarrelTip transform not set on " + weaponType.ToString() + "\nPlease set proper value in prefab");
            infractions++;
        }
        return (infractions <= 0 ? true : false);
    }

    public override void Initialize(Entity owner)
    {
        base.Initialize(owner);
        mag.Initialize(owner.faction);
    }
    #endregion Initialization

    #region UpdateMethods
    protected void Update ()
    {
        GunUpdate(true);
    }

    /// <summary>
    /// Handles gun states
    /// </summary>
    /// <param name="original">True if called from Update()</param>
    protected void GunUpdate (bool original)
    {
        switch (state)
        {
            case GunState.Idle:
                // Do Nothing
                break;

            case GunState.Reloading:
                if (Time.time >= reloadStarted + stats.reloadDelay)
                    EndReload();
                break;

            case GunState.Recovering:
                if (burstIndex == -1) // Fired once
                {
                    if (Time.time >= lastBurstStarted + stats.burstDelay && action1Released)
                    {
                        state = GunState.Idle;
                    }
                }
                else if (burstIndex == 0) // Last round of burst
                {
                    if (Time.time >= lastFired + stats.fireDelay && Time.time >= lastBurstStarted + stats.burstDelay)
                    {
                        state = GunState.Idle;
                        burstIndex = -1;
                        //Debug.LogError("Burst Over, " + Time.frameCount);
                    }
                }
                else // Mid-burst
                {
                    //Debug.Log("Mid-Burst, " + Time.frameCount+"\n" + Time.time + ", " + lastFired + ", " + stats.fireDelay);
                    if (Time.time >= lastFired + stats.fireDelay)
                    {
                        if (CanShoot())
                        {
                            float lastShot = lastFired;
                            lastFired += stats.fireDelay;
                            burstIndex--;
                            Shoot(Time.time - (lastShot + stats.fireDelay));
                        }
                        else
                            CancelBurst();
                    }
                }
                break;

            case GunState.Switching:
                if (Time.time >= timeEquipped + stats.equipDelay)
                    FinishEquipping();
                break;
        }
    }
    #endregion UpdateMethods

    #region Input
    public override void Equip ()
    {
        timeEquipped = Time.time;
        state = GunState.Switching;
        gameObject.SetActive(true);
    }

    public override void PutAway ()
    {
        InteruptActions();
        timeEquipped = -1;
        gameObject.SetActive(false);
    }

    public override void Action1 (bool triggered) // Pull trigger for gun
    {
        if (triggered)
        {
            if (CanBurst())
            {
                StartBurst(!action1Released);
                action1Released = false;
            }
            else if (reloadOnEmptyFire && mag.GetAmmo().x == 0)
            {
                if (CanReload())
                {
                    StartReload();
                    action1Released = true;
                }
            }
        }
        else
        {
            action1Released = true;
        }
    }
    
    public override void Action2 (bool triggered) // Reload by default (for now)
    {
        if (triggered)
        {
            action2Released = false;
            if (action2IsReload)
            {
                if (CanReload())
                {
                    StartReload();
                    action1Released = true;
                }
            }
        }
        else
        {
            action2Released = true;
        }
    }

    public override void InteruptActions ()
    {
        CancelBurst();
        CancelReload();
        CancelEquip();

        action1Released = true;
        action2Released = true;
    }

    public override void RemoveProjectile(GameObject proj)
    {
        activeBullets.Remove(proj);
    }
    #endregion Input

    #region Firing
    protected virtual void StartBurst (bool held)
    {
        if (stats.burstCount == 0 || stats.burstCount == 1) // Fire Normally
        {
            burstIndex = -1;
        }
        else if (stats.burstCount > 1) // Fire burst times
        {
            burstIndex = stats.burstCount-1;
        }
        else // Fire infinitely
        {
            burstIndex = 999;
        }

        if (held && stats.burstCount<2)
        {
            Shoot(Time.time - (lastBurstStarted+stats.burstDelay));
            lastBurstStarted += stats.burstDelay;
        }
        else
        {
            lastBurstStarted = Time.time;
            lastFired = Time.time;
            Shoot(0);
        }
    }
    
    protected virtual bool CancelBurst ()
    {
        if (burstIndex == 0) // Burst completed normally
        {
            burstIndex = -1;
            return true;
        }
        else if (burstIndex > 0) // Canceling Early
        {
            // Placeholder in-case needed
            burstIndex = -1;
            return true;
        }
        else // Not Bursting
            return false;
    }

    protected virtual void Shoot (float offset)
    {
        state = GunState.Recovering;

        int bulletsPerShot = mag.GetBulletsPerShot();
        for (int i=0; i<bulletsPerShot; i++)
        {
            ProjectileContainer container = mag.GetNextBullet(i==bulletsPerShot-1);
            float angleOffset = GetAccuracyModifier(stats.accuracy)*stats.spread*(Random.Range(0,2)*2-1);
            Quaternion rot = Quaternion.Euler(0,0, barrelTip.rotation.eulerAngles.z + angleOffset);
            GameObject bullet;
            if (offset > 0)
            {
                float dist = container.stats.speed * offset;
                bullet = ObjectPooler.ForceSetObject(barrelTip.position + rot*Vector3.right*dist, rot);
            }
            else
                bullet = ObjectPooler.ForceSetObject(barrelTip.position, rot);

            activeBullets.Add(bullet);
            bullet.GetComponent<Projectile>().Initialize(container, this);
        }

        if (stats.burstCount>1 && offset>stats.fireDelay) // Shoot multiple times per frame
        {
            Debug.LogWarning("Firing multiple per frame\n"+Mathf.Floor(offset/stats.fireDelay));
            GunUpdate(false);
        }
        else if (stats.burstCount<=1 && offset>stats.burstDelay)
        {
            Debug.LogWarning("Bursting multiple per frame\n" + Mathf.Floor(offset / stats.burstDelay));
            GunUpdate(false);
        }
        else if (player != null) // Play Shoot FX (Sounds, anim, flash, rumble)
        {
            StartRumble(new Rumble(Rumble.bullet));
        }
    }
    #endregion Firing

    #region Reload
    protected virtual void StartReload ()
    {
        reloadStarted = Time.time;
        state = GunState.Reloading;

        // Play reload FX (Sound, anim, rumble)
        if (player != null)
        {
            Rumble[] reloadAnim = Rumble.ScaleAnimation(Rumble.GetReloadAnim(), stats.reloadDelay - 0.05f);
            foreach (Rumble r in reloadAnim)
                StartRumble(r);
        }
    }

    protected virtual bool CancelReload ()
    {
        if (state == GunState.Reloading)
        {
            state = GunState.Idle;
            reloadStarted = -1;
            return true;
        }
        else
            return false;
    }

    protected virtual void EndReload ()
    {
        mag.Reload();
        state = GunState.Idle;
        reloadStarted = -1;
    }
    #endregion Reload

    #region Equip
    protected virtual bool CancelEquip ()
    {
        if (state == GunState.Switching)
        {
            timeEquipped = -1;
            return true;
        }
        else
            return false;
    }

    protected virtual void FinishEquipping ()
    {
        state = GunState.Idle;
    }
    #endregion Equip

    #region CheckMethods
    public virtual bool CanBurst ()
    {
        if (Time.time >= lastBurstStarted + stats.burstDelay &&
            CanShoot())
            return true;
        else
            return false;
    }

    public virtual bool CanShoot ()
    {
        if (mag.CanFire() &&
            (state == GunState.Idle || state == GunState.Recovering) &&
            Time.time >= lastFired + stats.fireDelay &&
            !IsBarrelBlocked())
            return true;
        else
            return false;
    }

    public virtual bool CanReload ()
    {
        Vector2 ammo = mag.GetAmmo();

        if (ammo.x < ammo.y &&
            (state == GunState.Idle || state == GunState.Recovering) &&
            true )
            return true;
        else
            return false;
    }

    /// <summary>
    /// Tests if anything is blocking the barrel
    /// </summary>
    /// <returns>True if blocked</returns>
    public virtual bool IsBarrelBlocked ()
    {
        return Physics2D.OverlapCircle(barrelTip.position, 0.2f, LayerMask.GetMask("Map"));// || Physics2D.CircleCast(barrelTip.position, 0.2f, barrelTip.right, 0.3f, LayerMask.GetMask("Map"));
    }
    #endregion CheckMethods

    #region General
    /// <param name="acc">Weapons accuracy, between 0 and 1</param>
    /// <returns>Affective spead, between 0 and 1</returns>
    protected float GetAccuracyModifier(float acc)
    {
        float rand = Random.Range(0f, 1f);
        if (rand <= 0.5f + acc/2f)
            return rand * (1f - acc); // slope = 1-acc
        else
            return 1f - (1f/(1f-acc) * (1f-rand)); // slope = 1/(1-acc)
    }
    #endregion General
}



[System.Serializable]
public class GunStats
{
    #region Properties
    [Tooltip("Time in s between bursts - Keep above .033")]
    public float burstDelay = 0.4f; // (s)
    [Tooltip("Used during bursts, time between shots")]
    public float fireDelay = 0; // (s)
    [Tooltip("Shots per burst, 0/1 -> no burst")]
    public int burstCount = 1; // (shots)
    [Tooltip("Maximum allowed bullet deviation in degrees")]
    public float spread = 5; // (degrees)
    public float accuracy = 1; // higher numbers, more centered shots
    [Tooltip("Time to reload")]
    public float reloadDelay = 1; // (s)
    [Tooltip("Time to equip")]
    public float equipDelay = 0.5f; // (s)
    #endregion Properties

    #region Constructors
    public GunStats ()
    {
        burstDelay = 0.4f;
        fireDelay = 0f;
        burstCount = 1;
        spread = 5;
        accuracy = 1;
        reloadDelay = 1;
        equipDelay = 0.5f;
    }

    public GunStats (float _burstDelay, float _fireDelay, int _burstCount, float _spread, float _accuracy, float _reloadDelay, float _equipDelay)
    {
        burstDelay = _burstDelay;
        fireDelay = _fireDelay;
        burstCount = _burstCount;
        spread = _spread;
        accuracy = _accuracy;
        reloadDelay = _reloadDelay;
        equipDelay = _equipDelay;
    }
    #endregion Constructors
}


#region Enums
public enum GunState
{
    Idle, Recovering, Reloading, Switching
}

public enum AmmoType
{
    None, // Infinite
    Physical, // Arrows, misc
    Bullet,
    Shell, // Shotguns
    Explosive,
    Energy
}

public enum DamageType
{
    Physical,
    Bullet,
    Explosive,
    Energy,
    Void
}

public enum FiringType
{
    Automatic,
    SemiAuto,
    Charged
}

public enum ReloadType
{
    Continuos,
    Clip,
    Shells,
    Overheat,
    Energy
}
#endregion Enums