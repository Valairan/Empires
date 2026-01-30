using Unity.Netcode;
using UnityEngine;

public class NetworkParent : NetworkBehaviour
{
    public string NetworkParentId;

    public void TriggerParenting(NetworkObjectReference networkObjectReference)
    {
        NetworkObject networkObject =
            NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectReference.NetworkObjectId];

        networkObject.transform.SetParent(transform);
        SynchronizeParentClientRpc(networkObjectReference);
    }

    [ClientRpc]
    private void SynchronizeParentClientRpc(NetworkObjectReference networkObjectReference)
    {
        NetworkObject networkObject =
            NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectReference.NetworkObjectId];

        networkObject.transform.SetParent(transform);
    }
}
