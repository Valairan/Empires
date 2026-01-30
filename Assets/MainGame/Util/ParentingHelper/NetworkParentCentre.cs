using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class NetworkParentCentre : NetworkBehaviour
{
    public NetworkVariable<ulong> OwnerId = new(writePerm: NetworkVariableWritePermission.Owner,
        readPerm: NetworkVariableReadPermission.Everyone);

    public List<NetworkParent> NetworkParents = new();

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        if (transform.parent != null)
        {
            Debug.LogError(
                $"Network Parent Centre: {transform.parent.name} | Can't have a parent (Must be a root object)");
            return;
        }

        NetworkParentCentre[] parentCentres = GetComponentsInChildren<NetworkParentCentre>();
        if (parentCentres.Length > 1)
        {
            Debug.LogError(
                $"Network Parent Centre: {transform.parent.name} | Can't have more than one Network Parent Centre on the same Network Object");
            return;
        }

        OwnerId.Value = NetworkManager.Singleton.LocalClientId;

        NetworkParents = GetComponentsInChildren<NetworkParent>().ToList();
    }

    public bool TryToParentNetworkObject(NetworkObjectReference networkObjectReference, string networkParentId)
    {
        if (!IsOwner) return false;

        NetworkParent networkParent = GetNetworkParent(networkParentId);
        if (networkParent == null) return false;

        networkParent.TriggerParenting(networkObjectReference);
        return true;
    }

    private NetworkParent GetNetworkParent(string networkParentId)
    {
        return NetworkParents.Find(x => x.NetworkParentId == networkParentId);
    }
}
