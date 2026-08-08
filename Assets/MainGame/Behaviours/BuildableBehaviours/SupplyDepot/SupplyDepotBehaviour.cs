using System;
using Unity.Netcode;
using UnityEngine;

public class SupplyDepotBehaviour : BuildableBehaviour, IInteractable
{
    public float InteractionDuration => 5f;
    public Item Interact(ulong interactor)
    {
        InteractWithSupplyDepot_ServerRpc(interactor);
        return Item;
    }




    [ServerRpc]
    public void InteractWithSupplyDepot_ServerRpc(ulong interactor)
    {
        if (!IsServer) return;
        if (interactor == ownerClientId.Value)
        {
            ClientRpcParams customParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { interactor }
                }
            };
            OpenWeaponMenu_ClientRpc(customParams);
        }

    }

    [ClientRpc]
    public void OpenWeaponMenu_ClientRpc(ClientRpcParams clientRpcParams = default)
    {
        interactWithMe?.Invoke();
    }

    public void takeDamage(DamageContext ctx)
    {
        throw new NotImplementedException();
    }
}
