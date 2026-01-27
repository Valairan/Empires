using Unity.Netcode;
using UnityEngine;

public class WeaponBehaviour : ItemBehaviour
{
    public virtual void OnAttackAnimationFinished()
    {

    }
    public virtual void OnAttackAnimationStarted()
    {

    }

    public Item Interact(GameObject interactor)
    {

        if (interactor.TryGetComponent(out PlayerController interactingPlayer))
        {
            attemptToInteract_ServerRpc(interactingPlayer.clientID);
        }
        return baseitem;
    }

    public Item respondToRaycast()
    {
        return baseitem;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.TryGetComponent(out IDamageable damageable))
        {
            damageable.takeDamage((MeleeWeapon)baseitem);
        }
    }

    [ServerRpc]
    void attemptToInteract_ServerRpc(ulong interactingPlayerId)
    {
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(interactingPlayerId, out var client))
            return;

        var playerObject = client.PlayerObject;
        if (playerObject == null)
            return;

        if (!playerObject.TryGetComponent(out PlayerController player))
            return;

        if (baseitem == null)
            return;

        float distance = Vector3.Distance(
            player.transform.position,
            transform.position
        );

        if (distance > 2.5f) // interaction range
            return;


        if (!player.PickUpItem(this))
            return;

        Debug.Log("The item has been picked up");

    }
}