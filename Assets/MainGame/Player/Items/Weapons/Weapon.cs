using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Base Weapon", menuName = "Items/Weapons/New Weapon")]
public class Weapon : Item
{
    public float treeDamage;
    public float oreDamage;
    public float playerDamage;
    public GameObject weaponPrefab_rb;
    public GameObject weaponPrefab_onplayer;

    void Awake()
    {
        stack = false;
    }
}