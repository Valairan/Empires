using UnityEngine;

public interface IInteractable
{
    public Item Interact(ulong interactor);
    public float InteractionDuration { get; }


}
