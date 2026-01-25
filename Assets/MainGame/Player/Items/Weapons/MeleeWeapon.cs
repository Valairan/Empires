using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Melee Weapon", menuName = "Items/Weapons/New Melee Weapon")]
public class MeleeWeapon : Weapon
{
    public GameObject weaponPrefab;
    public int firerate;
    public int magazine;
}