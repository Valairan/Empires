using System;
using UnityEngine;

public class InteractionHandler : MonoBehaviour
{
    public Item currentlyLookingAt;
    IInteractable currentInteractable;
    public float interactTimer = 0f;
    public bool interacting;
    public Action<bool, Vector3> onInteractableInView;
    public Action<float> onInteractionProgressChanged;
    public Action<Item, Vector3> onLookingAtChanged;
    public LayerMask whatToInclude;

    public void Init()
    {
        onLookingAtChanged += UiController.Singleton != null ? UiController.Singleton.setCurerntlyLookingAt : null;
        onInteractionProgressChanged += UiController.Singleton != null ? UiController.Singleton.setInteractionProgress : null;
        onInteractableInView += UiController.Singleton != null ? UiController.Singleton.displayInteractIcon : null;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void HandleTimedInteraction()
    {
        if (!interacting || currentInteractable == null)
        {
            interactTimer = 0f;
            return;
        }

        interactTimer += Time.deltaTime;

        float duration = currentInteractable.InteractionDuration;
        float progress = Mathf.Clamp01(interactTimer / duration);

        onInteractionProgressChanged?.Invoke(progress);

        if (interactTimer >= duration)
        {
            currentInteractable.Interact(gameObject);
            interactTimer = 0f;
            interacting = false; // require release to interact again
        }
    }

    public void checkForRaycasts(Transform startPosition)
    {
        if (Physics.SphereCast(startPosition.position, 1f, startPosition.forward, out RaycastHit hit, 50f, whatToInclude))
        {
            if (hit.transform.TryGetComponent(out IRaycastResponder responder))
            {
                Item item = responder.respondToRaycast();
                if (item != currentlyLookingAt)
                {
                    currentlyLookingAt = item;
                    onLookingAtChanged?.Invoke(item, hit.point);
                }
                if (hit.transform.TryGetComponent(out IInteractable interactable))
                {
                    currentInteractable = interactable;
                    onInteractableInView.Invoke(true, hit.point);
                }
                else
                {
                    currentInteractable = null;
                    onInteractableInView.Invoke(false, Vector3.zero);
                }
            }
            else
            {
                currentlyLookingAt = null;
                onLookingAtChanged?.Invoke(null, Vector3.zero);
            }
        }

    }
}


