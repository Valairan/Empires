using Unity.Netcode;

public class NetworkParentCentre : NetworkBehaviour
{
    public bool TryToParentNetworkObject(NetworkObjectReference networkObjectReference, NetworkParent parent)
    {
        if (!IsServer) return false;

        if (parent == null) { return false; }

        parent.TriggerParenting(networkObjectReference);
        return true;
    }

}
