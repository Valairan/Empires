using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Weapons/New Weapon")]
public class Weapon : Item
{
    void Awake()
    {
        stack = false;
    }
}