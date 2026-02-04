using JetBrains.Annotations;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class WeaponBehaviour : ItemBehaviour, IRaycastResponder, IInteractable
{
    public bool isAttacking = false;
    public Weapon baseWeapon;
    public Transform ik_target;
    public float InteractionDuration => 1f;

    public override void OnNetworkSpawn()
    {
        baseitem = baseWeapon;
        if (!IsLocalPlayer)
            enabled = false;
    }

    public void Init(ulong sender)
    {
        if (NetworkManager.Singleton.LocalClientId != sender)
        {
            enabled = false;
            return;
        }
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
        Debug.Log("after first check");

        NetworkObject playerObject = client.PlayerObject;
        if (playerObject == null)
            return;
        Debug.Log("after second check");

        if (!playerObject.TryGetComponent(out PlayerController player))
            return;

        Debug.Log("after third check");
        if (baseitem == null)
            return;

        Debug.Log("after fourth check");
        float distance = Vector3.Distance(
            player.transform.position,
            transform.position
        );

        Debug.Log("after final check");
        if (distance > 2.5f) // interaction range
            return;
        Debug.Log("before base.onpuit check");

        baseitem.OnPickup(interactingPlayerId, interactee);
    }
}