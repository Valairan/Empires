using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class TreeResourceBehaviour : BaseResourceBehaviour
{
    void Start()
    {

    }

    public override void takeDamage(Weapon damager)
    {
        float damage = damager.treeDamage;
        Debug.Log("The collision is being registered by " + damager.name);
        GameManager.Singleton.DamageTree_ServerRpc(damage, xCoordinate, yCoordinate);
    }


}
