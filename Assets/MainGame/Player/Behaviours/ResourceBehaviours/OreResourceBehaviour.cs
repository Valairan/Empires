using System;
using System.Data.Common;
using Unity.Netcode;
using UnityEngine;

public class OreResourceBehaviour : BaseResourceBehaviour
{
    public override void takeDamage(Weapon damager)
    {
        float damage = damager.oreDamage;
        ResourcesManager.Singleton.DamageOre_ServerRpc(damage, xCoordinate, yCoordinate);
    }


}
