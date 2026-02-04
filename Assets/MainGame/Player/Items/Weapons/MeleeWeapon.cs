using System;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "Melee Weapon", menuName = "Items/Weapons/New Melee Weapon")]
public class MeleeWeapon : Weapon
{
    void Awake()
    {
        WeaponType = WeaponType.melee;
    }
}