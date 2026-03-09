using Unity.Netcode;
using UnityEngine;

public class NetworkParent : NetworkBehaviour
{
    public void TriggerParenting(NetworkObjectReference networkObjectReference)
    {
        if (!IsServer) return;
        
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
    public void ClearParenting(NetworkObjectReference networkObjectReference)
    {
        if (!IsServer) return;
        
        NetworkObject networkObject =
            NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectReference.NetworkObjectId];

        networkObject.transform.SetParent(null);
        ClearParentClientRpc(networkObjectReference);
    }
    [ClientRpc]
    private void ClearParentClientRpc(NetworkObjectReference networkObjectReference)
    {
        NetworkObject networkObject =
            NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectReference.NetworkObjectId];
        networkObject.transform.SetParent(null);

    }
}
