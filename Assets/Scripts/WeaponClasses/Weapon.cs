using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    #region Properties
    protected PlayerController player;
    [Tooltip("Enum source under EntityEquipment.cs")]
    public WeaponType weaponType;
    public Faction faction;
    #endregion

    #region Initialization
    /*public void SetPlayer (PlayerController _player)
    {
        player = _player;
    }*/

    public virtual void Initialize(Entity owner)
    {
        faction = owner.faction;
        player = owner.container.controller as PlayerController;
    }

    /// <summary>
    /// Runs tests on the weapon to determine if it's set up properly
    /// </summary>
    public virtual bool TestValidity()
    {
        return true;
    }
    #endregion

    #region Input
    /// <summary> Activates the weapon </summary>
    public abstract void Equip();

    /// <summary> Deactivates the weapon </summary>
    public abstract void PutAway();

    /// <summary> Calls Action 1 for the weapon </summary>
    /// <param name="triggered"> true to trigger </param>
    public abstract void Action1 (bool triggered);

    /// <summary> Calls Action 2 for the weapon </summary>
    /// <param name="triggered"> true to trigger </param>
    public abstract void Action2 (bool triggered);

    /// <summary> Stops all possible actions from occuring </summary>
    public abstract void InteruptActions ();

    /// <summary>
    /// Removes the provided bullet from an active-bullets array, if one exists
    /// </summary>
    /// <param name="bullet">The bullet to be removed</param>
    public abstract void RemoveProjectile (GameObject proj);
    #endregion

    #region General
    protected void StartRumble(Rumble _rumble)
    {
        player.StartRumble(_rumble); // Modify to call Rumble Manager later
    }
    #endregion
}