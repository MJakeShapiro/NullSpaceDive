using UnityEngine;

[System.Serializable]
public class SimpleMag : Magazine
{
    public ProjectileContainer bullet;
    public int bulletsPerShot = 1;
    public int magSize;
    protected int maxAmmo;
    [SerializeField]
    protected int curAmmo;

    public override void Update ()
    {
        
    }

    public override void Initialize ()
    {
        SetStats();
        curAmmo = maxAmmo;
    }

    public override void SetStats ()
    {
        // Do other stuff here, once ammo capactiy mods are a thing
        maxAmmo = magSize;
        // Also update mods if needed? IDK where to put that yet
    }

    public override bool CanFire ()
    {
        if (curAmmo > 0)
            return true;
        else
            return false;
    }

    public override ProjectileContainer GetNextBullet (bool boo)
    {
        if (curAmmo <= 0)
            return null;
        else
        {
            if (boo)
                curAmmo--;
            return bullet;
        }
    }

    public override ProjectileContainer[] GetAllBullets ()
    {
        ProjectileContainer[] bullets = new ProjectileContainer[1];
        bullets[0] = bullet;
        return bullets;
    }

    public override int GetBulletsPerShot ()
    {
        return bulletsPerShot;
    }

    public override Vector2 GetAmmo ()
    {
        return new Vector2(curAmmo, maxAmmo);
    }

    public override bool Reload ()
    {
        if (curAmmo == maxAmmo)
            return false;

        curAmmo = maxAmmo;
        return true;
    }

    public override bool Reload (int n)
    {
        if (curAmmo == maxAmmo)
            return false;

        curAmmo = Mathf.Clamp(curAmmo + n, 0, maxAmmo);
        return true;
    }
}