using UnityEngine;

public interface IInteractable
{
    public Item Interact(GameObject interactor);
    public float InteractionDuration { get; }


}
