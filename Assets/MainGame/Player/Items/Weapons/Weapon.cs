using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Base Weapon", menuName = "Items/Weapons/New Weapon")]
public class Weapon : Item
{
    public float treeDamage;
    public float oreDamage;
    public float playerDamage;
    void Awake()
    {
        stack = false;
    }
}