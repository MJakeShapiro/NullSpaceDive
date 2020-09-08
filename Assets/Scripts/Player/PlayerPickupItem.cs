using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;

/// <summary>
/// Most of this is temporary for now, but it gets the job done!
/// Uses an array to handle chests mechanics
/// </summary>
public class PlayerPickupItem : MonoBehaviour
{
    public GameObject[] weapon = new GameObject[1];

    bool deactivateOnGet = false;
    bool destroyOnGet = false;

    void OnTriggerEnter2D(Collider2D col)
    {
        Entity other = col.GetComponentInParent<Entity>();
        if (other != null && other.container.controller is PlayerController)
        {
            OnPlayerContact(other.container.controller as PlayerController, other);
        }
    }

    void OnPlayerContact (PlayerController pc, Entity entity)
    {
        RemovePlayersCurrentItem(pc);

        GameObject wep = weapon[Random.Range(0, weapon.Length-1)];
        GiveItemToPlayer(pc, entity, wep);

        if (destroyOnGet)
        {
            Destroy(gameObject);
        }
        else if (deactivateOnGet)
        {
            gameObject.SetActive(false);
        }
    }

    void RemovePlayersCurrentItem (PlayerController pc)
    {
        Destroy(pc.activeWeapon.gameObject);
    }

    void GiveItemToPlayer (PlayerController pc, Entity entity, GameObject wepObj)
    {
        GameObject gun = Instantiate(wepObj, entity.container.aiming.rotator);
        Weapon newWep = gun.GetComponent<Weapon>();
        if (newWep != null)
        {
            newWep.transform.localPosition = Vector3.zero;
            newWep.Initialize(entity);
            pc.activeWeapon = newWep;
        }
        else
        {
            Debug.LogError("Couldn't find weapon for " + name + ", uh oh!");
            Destroy(gun);
        }
    }
}