using System;
using Unity.Netcode;
using UnityEditor.Build.Content;
using UnityEngine;

[RequireComponent(typeof(Health))]

public class BuildableBehaviour : ItemBehaviour<Machine>, IRaycastResponder, IDamageable
{

    public Health health;
    public MachineState state;
    public Transform[] placementPoints;
    public NetworkVariable<ulong> ownerClientId = new NetworkVariable<ulong>(ulong.MaxValue);

    public virtual Item respondToRaycast(ulong interactor)
    {

        return baseitem;
    }

    public virtual Item registerToPlayer()
    {
        return Item;
    }

    public void takeDamage(DamageContext ctx)
    {
        throw new NotImplementedException();
    }

    public Action interactWithMe;

}

public enum MachineState
{
    preview,
    placed
}

