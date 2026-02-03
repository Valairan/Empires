using UnityEngine;

public class ResourceDropBehaviour : ItemBehaviour, IRaycastResponder, IInteractable
{

    float IInteractable.InteractionDuration => 0f;

    public Item Interact(ulong interactor)
    {
        return baseitem;
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.transform.TryGetComponent(out PlayerController controller))
        {
            baseitem.OnPickup(controller.OwnerClientId, this.NetworkObjectId);
        }
    }

    public Item respondToRaycast()
    {
        return baseitem;
    }



}
