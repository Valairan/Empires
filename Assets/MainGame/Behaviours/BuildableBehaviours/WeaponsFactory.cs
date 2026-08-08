using UnityEngine;

public class WeaponsFactory : BuildableBehaviour, IInteractable
{
    public float InteractionDuration => 1f;

    public bool placed = false;
    public override void OnNetworkSpawn()
    {
        placed = true;
        BindUI();
    }

    public void BindUI()
    {

    }

    public Item Interact(ulong interactor)
    {
        if (!placed) return null;
        return baseitem;
    }
}
