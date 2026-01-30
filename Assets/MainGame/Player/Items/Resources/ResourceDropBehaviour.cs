using Unity.Netcode;
using UnityEngine;

public class ResourceDropBehaviour : ItemBehaviour, IRaycastResponder, IInteractable
{

    float IInteractable.InteractionDuration => 0f;

    public Item Interact(GameObject interactor)
    {
        if (interactor.TryGetComponent<PlayerController>(out PlayerController controller))
        {
            attemptToPickUpResource_ServerRpc(controller.clientID);
        }
        return baseitem;
    }
    [ServerRpc]
    void attemptToPickUpResource_ServerRpc(ulong clientid)
    {
        if (!IsServer) return;
        if (Vector3.Distance(NetworkGamePropertiesStorage.Singleton.spawnedPlayers[clientid].transform.position, transform.position) > 20)
        {
            if (NetworkGamePropertiesStorage.Singleton.spawnedPlayers[clientid].TryGetComponent<PlayerController>(out PlayerController controller))
            {
                //controller.playerInventory.addItem(baseItem);    
            }
        }
    }

    public Item respondToRaycast()
    {
        return baseitem;
    }

    public void OnCollisionEnter()
    {

    }

}
