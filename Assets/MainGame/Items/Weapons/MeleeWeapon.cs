using System;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "Melee Weapon", menuName = "Empires/Weapons/New Melee Weapon")]
public class MeleeWeapon : Weapon
{
    public float swingSpeed = 1f;   // Swings per second

    void Awake()
    {
        WeaponType = WeaponType.melee;
    }
}
