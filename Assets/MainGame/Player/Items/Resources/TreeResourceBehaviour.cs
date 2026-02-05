using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class TreeResourceBehaviour : BaseResourceBehaviour
{


    public override void takeDamage(Weapon damager)
    {

        float damage = damager.treeDamage;
        GameManager.Singleton.DamageTree_ServerRpc(damage, xCoordinate, yCoordinate);
    }


}
