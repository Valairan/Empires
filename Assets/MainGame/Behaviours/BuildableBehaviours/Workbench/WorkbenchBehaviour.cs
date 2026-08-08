using Unity.Netcode;
using UnityEngine;

public class WorkbenchBehaviour : BuildableBehaviour, IInteractable
{
    public float InteractionDuration => 1f;

    public Item Interact(ulong interactor)
    {
        attemptToInteract_ServerRpc(interactor, NetworkObjectId);
        return baseitem;
    }

    [ServerRpc(RequireOwnership = false)]
    void attemptToInteract_ServerRpc(ulong interactingPlayerId, ulong interactee)
    {
        if (NetworkObject.OwnerClientId != interactingPlayerId)
        {
            return;
        }
        if (!NetworkManager.Singleton.ConnectedClients
            .TryGetValue(interactingPlayerId, out NetworkClient client))
            return;

        NetworkObject playerObject = client.PlayerObject;
        if (playerObject == null)
            return;

        if (baseitem == null)
            return;

        float distance = Vector3.Distance(
            playerObject.transform.position,
            transform.position
        );

        if (distance > 1f)
            return;



    }

    public override Item respondToRaycast(ulong interactor)
    {
        if (NetworkObject.OwnerClientId != interactor)
        {
            return null;
        }
        return baseitem;
    }





}
