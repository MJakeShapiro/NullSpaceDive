using UnityEngine;

/// <summary>
/// Most of this is temporary for now, but it gets the job done!
/// Uses an array to handle chests mechanics
/// </summary>
public class PlayerPickupItem : MonoBehaviour
{
    public WeaponType[] weapon = new WeaponType[1];

    public bool deactivateOnGet = false;
    public bool destroyOnGet = false;

    private void Awake()
    {
        foreach (WeaponType wt in weapon)
            if (wt == WeaponType.Null)
                Debug.LogWarning($"Uh oh! WeaponType cant be null for '{name}' PickupItem!");
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Entity other = col.GetComponentInParent<Entity>();
        if (other != null && other.container.controller is PlayerController)
        {
            OnPlayerContact(other.container.equipment);
        }
    }

    void OnPlayerContact (EntityEquipment equipment)
    {
        if (equipment.PickupWeapon(weapon[Random.Range(0, weapon.Length)]))
        {
            if (destroyOnGet)
                Destroy(gameObject);
            else if (deactivateOnGet)
                gameObject.SetActive(false);
        }
    }
}