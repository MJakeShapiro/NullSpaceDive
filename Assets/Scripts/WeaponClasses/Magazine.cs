using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Magazine : MonoBehaviour
{
    public abstract void Update(); // Misc per-frame functionality
    public abstract void Initialize(); // Sets ammo to max, etc
    public abstract void SetStats(); // Applies latests mods, such as ammo capacity
    public abstract bool CanFire(); // Returns true if able to fire
    public abstract int GetBulletsPerShot(); // Returns number of bullets fired each shot
    public abstract ProjectileContainer GetNextBullet(bool boo); // Returns the next bullet to be fired. True if consuming a bullet
    public abstract ProjectileContainer[] GetAllBullets(); // Returns all available bullets
    public abstract Vector2 GetAmmo(); // Returns current ammo, and max ammo
    public abstract bool Reload(); // Fully reloads weapon
    public abstract bool Reload(int n); // Partially reloads weapon, by n ammount
}