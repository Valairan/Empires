using System;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class WeaponsFactory : MachineBehaviour, IInteractable
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
        UiController.Singleton.toggleWeaponSelector();
        return baseitem;
    }
}
