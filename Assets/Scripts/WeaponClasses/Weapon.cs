using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    protected PlayerController player;

    public void SetPlayer (PlayerController _player)
    {
        player = _player;
    }

    public void StartRumble (Rumble _rumble)
    {
        player.StartRumble(_rumble);
    }

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
}
