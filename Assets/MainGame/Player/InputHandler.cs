using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : NetworkBehaviour
{
    private InGame ingameInputActions;


    public Action<Vector2> Move;
    public Action<Vector2> Look;
    public Action Attack;
    public Action<bool> Interact;
    public Action<bool> Jump;
    public Action<bool> Sneak;
    public Action Build;
    public Action Previous;
    public Action Next;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        ingameInputActions = new InGame();
        ingameInputActions.Player.Move.performed += ctx => Move?.Invoke(ctx.ReadValue<Vector2>());
        ingameInputActions.Player.Move.canceled += _ => Move?.Invoke(Vector2.zero);
        ingameInputActions.Player.Look.performed += ctx => Look?.Invoke(ctx.ReadValue<Vector2>()); ;
        ingameInputActions.Player.Look.canceled += _ => Look?.Invoke(Vector2.zero); ;
        ingameInputActions.Player.Attack.performed += ctx => Attack?.Invoke();

        ingameInputActions.Player.Interact.performed += ctx => Interact?.Invoke(true);
        ingameInputActions.Player.Interact.canceled += ctx => Interact?.Invoke(false);

        ingameInputActions.Player.Jump.performed += ctx => Jump?.Invoke(true);
        ingameInputActions.Player.Jump.canceled += ctx => Jump?.Invoke(false);

        ingameInputActions.Player.Sneak.performed += ctx => Sneak?.Invoke(true);
        ingameInputActions.Player.Sneak.canceled += ctx => Sneak?.Invoke(false);
        ingameInputActions.Player.Previous.started += ctx => Previous?.Invoke();
        ingameInputActions.Player.Next.started += ctx => Next?.Invoke();

        ingameInputActions.Player.Build.performed += _ => Build.Invoke();

    }
    public void enableInputs()
    {
        ingameInputActions.Enable();
    }
    public void disableInputs()
    {
        ingameInputActions.Disable();
    }



}
