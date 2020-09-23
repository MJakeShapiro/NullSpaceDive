﻿using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

public class EntityEquipment : MonoBehaviour
{
    #region Properties
    Entity.EntityReferenceContainer container;

    public List<Weapon> weapons = new List<Weapon>(1);
    public int maxWeapons = 1;

    public string item = "None";
    public int itemAmmount = 0;

    [SerializeField][ReadOnly]
    protected int weaponIndex = 0;
    #endregion Properties

    #region Initialization
    private void Awake()
    {
        foreach (Weapon wep in weapons)
            wep.Initialize(container.entity);
    }

    /// <summary>
    /// Sets the Entity references to other Entity componenets
    /// </summary>
    /// <param name="_container">Reference Container Class</param>
    public virtual void SetEntityReference(Entity.EntityReferenceContainer _container)
    {
        container = _container;
    }
    #endregion

    #region UpdateMethods
    void Update()
    {
        // foreach () ~> WeaponCD.Update();
    }
    #endregion

    #region Input
    public virtual void TriggerAction1 (bool triggered)
    {
        if (weapons.Count > 0)
            weapons[weaponIndex].Action1(triggered);
    }

    public virtual void TriggerAction2(bool triggered)
    {
        if (weapons.Count > 0)
            weapons[weaponIndex].Action2(triggered);
    }

    /// <summary>
    /// Equips the weapon at the provided index
    /// </summary>
    /// <param name="index">New weaponIndex</param>
    /// <returns>true if successful</returns>
    public virtual bool EquipWeapon (int index)
    {
        if (weaponIndex == index || index >= weapons.Count)
            return false;
        weapons[weaponIndex].PutAway();
        weaponIndex = index;
        weapons[weaponIndex].Equip();
        return true;
    }

    /// <summary>
    /// Scrolls through the current guns
    /// </summary>
    /// <returns>true if successful</returns>
    public virtual bool EquipNextWeapon ()
    {
        if (weapons.Count <= 1)
            return false;
        weapons[weaponIndex].PutAway();
        if (++weaponIndex>=weapons.Count) weaponIndex=0;
        weapons[weaponIndex].Equip();
        return true;
    }

    /// <summary>
    /// Scrolls through the current guns
    /// </summary>
    /// <returns>true if successful</returns>
    public virtual bool EquipPreviousWeapon()
    {
        if (weapons.Count <= 1)
            return false;
        weapons[weaponIndex].PutAway();
        if (--weaponIndex<0) weaponIndex = weapons.Count-1;
        weapons[weaponIndex].Equip();
        return true;
    }

    public virtual bool PickupWeapon (WeaponType type)
    {
        if (type==WeaponType.Null)
        {
            Debug.LogWarning("Attempting to pick up WeaponType.Null, who fucked that one up??");
            return false;
        }
        foreach (Weapon wep in weapons)
            if (wep.weaponType == type) // if picking up same type of weapon
                return false;

        if (weapons.Count >= maxWeapons)
        {
            DestroyWeapon(weaponIndex);
            weapons.Add(InstantiateNewWeapon(type));
            weapons[weaponIndex].Equip();
        }
        else
        {
            weapons.Add(InstantiateNewWeapon(type));
            EquipWeapon(weapons.Count - 1);
        }
        return true;
    }

    public virtual bool PickupItem (string _name, int _ammount = 1)
    {
        Debug.Log(gameObject.name + " picked up " + _ammount + " " + _name);
        item = _name;
        itemAmmount = _ammount;
        return true;
    }
    #endregion

    #region Equipment Management
    protected virtual Weapon InstantiateNewWeapon(WeaponType type)
    {
        GameObject obj = EquipmentManager.GetWeapon(type);
        if (container?.aiming?.rotator == null)
        {
            Debug.Log("Trying to spawn a gun without a place to put it, idek how to handle this. Good luck?");
            obj = Instantiate(obj, container.entity.transform);
        }
        else
            obj = Instantiate(obj, container.aiming.rotator);

        Weapon newWep = obj.GetComponent<Weapon>();
        if (newWep == null)
            Debug.LogError("Couldn't find a weapon! How did thise get past Staiks automated testing system??");
        else
        {
            newWep.transform.localPosition = Vector3.zero;
            newWep.Initialize(container.entity);
        }

        return newWep;
    }

    protected virtual void DestroyWeapon (int index)
    {
        weapons[index].PutAway();
        Destroy(weapons[index].gameObject);
    }
    #endregion

    #region CheckMethods
    public bool IsHoldingWeapon ()
    {
        return weapons.Count > 0;
    }
    #endregion
}



public class EquipmentManager
{
    #region StaticMethods
    public static GameObject GetWeapon(WeaponType type)
    {
        GameObject obj = Resources.Load("Weapons/"+type.ToString(), typeof(GameObject)) as GameObject;
        return obj;
    }
    
    public static bool TestAllWeapons (out int infractions, out int totalCases)
    {
        infractions = 0;
        totalCases = System.Enum.GetNames(typeof(WeaponType)).Length - 1;

        for (int i=1; i<totalCases+1; i++)
        {
            GameObject obj = Resources.Load("Weapons/"+((WeaponType)i).ToString(), typeof(GameObject)) as GameObject;
            if (obj != null)
            {
                Weapon wep = obj.GetComponent<Weapon>();
                if (wep != null)
                {
                    if (!wep.TestValidity())
                        infractions++;
                    //else passed!
                }
                else
                    Debug.LogWarning($"Infraction {++infractions}\nResources/Weapons/{(WeaponType)i} not a valid Weapon");
            }
            else
                Debug.LogWarning($"Infraction {++infractions}\nResources/Weapons/{(WeaponType)i} not a valid GameObject");
        }

        return infractions<=0;
    }
    #endregion StaticMethods
}

#region Enums
public enum WeaponType
{
    Null = 0,
    FAMAS,
    MarinePistol,
    M4_Super,
    SCAR
}
#endregion