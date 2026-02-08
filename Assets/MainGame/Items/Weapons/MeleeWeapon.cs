using System;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "Melee Weapon", menuName = "Items/Weapons/New Melee Weapon")]
public class MeleeWeapon : Weapon
{
    public float swingSpeed = 1f;   // Swings per second

    void Awake()
    {
        WeaponType = WeaponType.melee;
        isAutomatic = false;        // Melee attacks are manual
    }
}
