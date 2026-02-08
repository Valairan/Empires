using Unity.Netcode;
using UnityEngine;

public class WeaponBehaviour : ItemBehaviour, IRaycastResponder, IInteractable
{
    public bool isAttacking = false;
    public Weapon baseWeapon;
    public WeaponState state;
    public Transform ik_target;
    public float InteractionDuration => 1f;

    public override void OnNetworkSpawn()
    {
        baseitem = baseWeapon;
        if (!IsOwner)
            enabled = false;
    }

    public void store()
    {
        transform.localPosition = baseWeapon.storedPosition;
        transform.localRotation = Quaternion.Euler(baseWeapon.storedRotation);
        transform.localScale = baseWeapon.storedScale;
        state = WeaponState.stored;
    }
    public void equip(ulong sender)
    {
        transform.localPosition = baseWeapon.position;
        transform.localRotation = Quaternion.Euler(baseWeapon.rotation);
        transform.localScale = baseWeapon.scale;
    }



    public Item Interact(ulong interactor)
    {

        attemptToInteract_ServerRpc(interactor, NetworkObjectId);
        return baseitem;
    }

    public Item respondToRaycast()
    {
        return baseitem;
    }

    [ServerRpc(RequireOwnership = false)]
    void attemptToInteract_ServerRpc(ulong interactingPlayerId, ulong interactee)
    {
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(interactingPlayerId, out NetworkClient client))
            return;
        NetworkObject playerObject = client.PlayerObject;
        if (playerObject == null)
            return;


        if (baseitem == null)
            return;

        float distance = Vector3.Distance(
            NetworkManager.Singleton.ConnectedClients[interactingPlayerId].PlayerObject.transform.position,
            transform.position
        );
        if (distance > 2.5f) // interaction range
            return;
        ItemPickupContext ctx = new ItemPickupContext
        {
            inventory = NetworkManager.Singleton.ConnectedClients[interactingPlayerId]
        .PlayerObject.GetComponent<IInventory>(),
            inworld = this
        };
        baseitem.OnPickup(ctx);
    }
}

public enum WeaponState
{
    stored,
    equipped,
    inworld
}