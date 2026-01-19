using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Weapons/New Melee Weapon")]
public class MeleeWeapon : Weapon
{
    public GameObject ingameWeapon;
    public GameObject equippedWeapon;
    public int firerate;
    public int magazine;
}