using System;
using Unity.Netcode;
using UnityEngine;

public class TreeResourceBehaviour : BaseResourceBehaviour
{
    void Start()
    {

    }

    public override void takeDamage(Weapon damager)
    {
        health.amount -= damager.treeDamage;
    }

    [ServerRpc]
    public void DamageTree_ServerRpc()
    {

    }


}
