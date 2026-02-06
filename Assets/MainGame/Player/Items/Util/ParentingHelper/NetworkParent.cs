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
        
        networkObject.TryGetComponent(out WeaponBehaviour wb);
        if (wb) wb.Init(networkObject.OwnerClientId);
    }
}
