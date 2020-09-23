using UnityEngine;

public class PickupWeapon : Interactable
{
    #region Properties
    [Header("PickupWeapon")]
    [Tooltip("Weapon type to give the player")]
    public WeaponType weapon;
    //public GameObject pickupDisplay; // TODO
    #endregion Properties

    #region Initialization
    protected override void Awake()
    {
        base.Awake();
        #region Debug
#if UNITY_EDITOR
        if (weapon == WeaponType.Null)
            Debug.LogWarning($"Uh oh! WeaponType cant be null for '{name}' PickupWeapon!");
#endif
        #endregion
    }

    private void OnEnable()
    {
        //pickupDisplay.Initialize(); Temp - for later use
    }
    #endregion Initialization

    #region EventMethods
    protected override void OnHighlight ()
    {
        //pickupDisplay.Show();
    }

    protected override void OnUnhighlight ()
    {
        //pickupDisplay.Hide();
    }

    protected override bool OnSelected (Entity playerEntity)
    {
        return playerEntity.container.equipment.PickupWeapon(weapon);
    }
    #endregion
}