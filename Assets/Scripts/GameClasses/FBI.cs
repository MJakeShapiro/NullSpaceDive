using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Federal Booty Inspector - aka - Official Loot Manager
/// </summary>
public class FBI
{
    #region Properties

    #endregion

    #region Statics
    public static Loot ShowMeTheBooty (LootTable table)
    {
        return new Loot();
    }
    #endregion

    #region Internal

    #endregion

    #region Enums
    public enum LootTable
    {
        Crate,
        Chest,
        Minion,
        Miniboss
    }

    public enum LootType
    {
        None = 0,
        Weapon_1s,
        Weapon_2s,
        Weapon_3s,
        Item_1s,
        Item_2s,
        Item_3s,
        Mod_1,
        Mod_2,
        Mod_3,
        Ammo_1,
        Ammo_2,
        Ammo_spread,
        Health_small, // Partially fills container
        Health_medium, // Fills current container
        Health_full, // Fills current and next container (Or next if current is full)
    }
    #endregion
}



/// <summary>
/// Container that passes generated loot to the game
/// </summary>
public struct Loot
{
    #region Properties
    List<FBI.LootType> contents;
    #endregion

    #region Constructors

    #endregion

    #region Methods 

    #endregion
}



/// <summary>
/// Container that stores ammo
/// </summary>
public struct AmmoDrop
{
    #region Properties
    public int amount;
    public AmmoType type;
    #endregion

    #region Constructors
    /// <summary>
    /// Generates an AmmoDrop
    /// </summary>
    public AmmoDrop(int _amount, AmmoType _type)
    {
        amount = _amount;
        type = _type;
    }

    /// <summary>
    /// Generates an AmmoDrop with ammo between the provided limits
    /// </summary>
    public AmmoDrop (int min, int max, AmmoType _type)
    {
        amount = Random.Range(min, max+1);
        type = _type;
    }
    #endregion
}