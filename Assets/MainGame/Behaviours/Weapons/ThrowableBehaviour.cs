using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class ThrowableWeaponBehaviour : WeaponBehaviour<ThrowableWeapon>
{
    public ThrowableWeapon baseitem
    {
        get => (ThrowableWeapon)base.baseitem;
        set => base.baseitem = value;
    }

    
}