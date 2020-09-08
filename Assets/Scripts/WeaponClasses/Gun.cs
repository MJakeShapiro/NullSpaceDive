﻿using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Weapon
{
    public FiringType firingType = FiringType.Automatic;
    public AmmoType ammoType = AmmoType.Bullet;
    public ReloadType reloadType = ReloadType.Clip;
    public GunStats stats;
    //[SerializeField]
    public Magazine mag;

    public GunState state = GunState.Idle;

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

    protected void Awake ()
    {
        if (stats.burstCount == 1)
            stats.burstCount = 0;
    }

    public override void Initialize(Entity owner)
    {
        base.Initialize(owner);
        mag.Initialize(owner.faction);
    }

    protected void Update ()
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
                    if (Time.time >= lastBurstStarted + stats.burstDelay)
                    {
                        state = GunState.Idle;
                    }
                }
                else if (burstIndex == 0) // Last round of burst
                {
                    if (Time.time >= lastFired + stats.fireDelay && Time.time >= lastBurstStarted + stats.burstDelay)
                    {
                        state = GunState.Idle;
                        burstIndex=-1;
                    }
                }
                else // Mid-burst
                {
                    if (Time.time >= lastFired+stats.fireDelay)
                    {
                        if (CanShoot())
                        {
                            float lastShot = lastFired;
                            Shoot();
                            lastFired = lastShot + stats.fireDelay;
                            burstIndex--;
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

    public override void Equip ()
    {
        timeEquipped = Time.time;
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
                action1Released = false;
                StartBurst();
            }
            else if (reloadOnEmptyFire && mag.GetAmmo().x == 0)
            {
                if (CanReload())
                {
                    action1Released = false;
                    StartReload();
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
                    StartReload();
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
    }

    public override void RemoveProjectile(GameObject proj)
    {
        activeBullets.Remove(proj);
    }

    protected virtual void StartBurst ()
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

        lastBurstStarted = Time.time;
        Shoot();
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

    protected virtual void Shoot ()
    {
        lastFired = Time.time;
        state = GunState.Recovering;

        int bulletsPerShot = mag.GetBulletsPerShot();
        for (int i=0; i<bulletsPerShot; i++)
        {
            ProjectileContainer container = mag.GetNextBullet(i==bulletsPerShot-1);
            float angleOffset = (Random.value-0.5f)*stats.spread;
            Quaternion rot = Quaternion.Euler(0,0, barrelTip.rotation.eulerAngles.z + angleOffset);
            GameObject bullet = ObjectPooler.ForceSetObject(barrelTip.position, rot);
            activeBullets.Add(bullet);
            bullet.GetComponent<Projectile>().Initialize(container, this);
        }

        // Play Shoot FX (Sounds, anim, flash, rumble)
        if (player != null)
        {
            StartRumble(new Rumble(Rumble.bullet));
            StartRumble(new Rumble(Rumble.bullet2));
        }
    }

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

    public virtual bool CanBurst ()
    {
        if (mag.CanFire() &&
            (state == GunState.Idle) &&
            Time.time >= lastFired + stats.fireDelay &&
            Time.time >= lastBurstStarted + stats.burstDelay )
            return true;
        else
            return false;
    }

    public virtual bool CanShoot ()
    {
        if (mag.CanFire() &&
            (state == GunState.Idle || state == GunState.Recovering) &&
            Time.time >= lastFired + stats.fireDelay)
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
}


public enum GunState
{
    Idle, Recovering, Reloading, Switching
}



[System.Serializable]
public class GunStats
{
    public float burstDelay = 0; // (s)
    public float fireDelay = 0.4f; // (s)
    public int burstCount = 1; // (shots)
    public float spread = 5; // (degrees)
    public float accuracy = 1; // higher numbers, more centered shots
    public float reloadDelay = 1; // (s)
    public float equipDelay = 0.5f; // (s)

    public GunStats ()
    {
        burstDelay = 0;
        fireDelay = 0.4f;
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
}

public enum AmmoType
{
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