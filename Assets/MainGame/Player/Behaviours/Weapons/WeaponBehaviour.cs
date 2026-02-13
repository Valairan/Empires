using System;
using Unity.Netcode;
using UnityEngine;

//
// NON-GENERIC RUNTIME BASE
// Unity & Netcode talk to this.
//
public abstract class WeaponBehaviour
    : ItemBehaviour<Weapon>, IRaycastResponder, IInteractable
{
    public bool isAttacking = false;
    public WeaponState state;
    public Transform ik_target;

    public float InteractionDuration => 1f;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            enabled = false;
    }

    public virtual void store()
    {
        transform.localPosition = baseitem.storedPosition;
        transform.localRotation = Quaternion.Euler(baseitem.storedRotation);
        transform.localScale = baseitem.storedScale;
        state = WeaponState.stored;
    }

    public virtual void equip(ulong sender)
    {
        transform.localPosition = baseitem.position;
        transform.localRotation = Quaternion.Euler(baseitem.rotation);
        transform.localScale = baseitem.scale;
        state = WeaponState.equipped;
    }

    [ServerRpc]
    public virtual void Attack_ServerRpc(Vector3 target)
    {
        // Base weapon does nothing.
        // Concrete classes override this.
    }

    // -------------------------
    // Interaction Logic
    // -------------------------

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

        if (distance > 2.5f)
            return;

        ItemPickupContext ctx = new ItemPickupContext
        {
            inventory = playerObject.GetComponent<IInventory>(),
            inworld = this
        };

        baseitem.OnPickup(ctx);
    }
}

//
// GENERIC TYPED LAYER
// Only adds strong typing — no duplicate logic.
//
public abstract class WeaponBehaviour<T>
    : WeaponBehaviour
    where T : Weapon
{
    protected T TypedItem => (T)baseitem;
}

//
// ENUM
//
public enum WeaponState
{
    stored,
    equipped,
    inworld
}


public interface IWeaponTriggerable
{
    void TriggerPressed(Vector3 aimPoint);
    void TriggerReleased();
    bool CanFire();
}
public interface IWeaponUpdatable
{
    public void switchFiremode();
    public void reload();
    public void UpdateWeapon(Vector3 aimPoint);
}