using System;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class WeaponsFactory : MachineBehaviour, IInteractable
{
    public float InteractionDuration => 2f;

    public bool placed = false;
    public override void OnNetworkSpawn()
    {
        placed = true;
    }

    public void BindUI()
    {

    }



    public Item Interact(GameObject interactor)
    {
        if (!IsLocalPlayer) return null;
        UiController.Singleton.toggleBuildMenu();

        return baseitem;
    }
}
