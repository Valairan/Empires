using System;
using Unity.Netcode;
using UnityEngine;

public class InputHandler : NetworkBehaviour
{
    private InGame ingameInputActions;


    public Action<Vector2> Move;
    public Action<Vector2> Look;
    public Action<bool> Aim;
    public Action<bool> Attack;
    public Action<bool> Reload;
    public Action<bool> Interact;
    public Action<bool> Jump;
    public Action<bool> Sneak;
    public Action Build;
    public Action Rotate;
    public Action Cancel;
    public Action Previous;
    public Action Next;
    public Action Drop;
    public Action<int> Equip;
    public Action Stash;
    public Action Inventory;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        ingameInputActions = new InGame();
        ingameInputActions.Player.Move.performed += ctx => Move?.Invoke(ctx.ReadValue<Vector2>());
        ingameInputActions.Player.Move.canceled += _ => Move?.Invoke(Vector2.zero);
        ingameInputActions.Player.Look.performed += ctx => Look?.Invoke(ctx.ReadValue<Vector2>()); ;
        ingameInputActions.Player.Look.canceled += _ => Look?.Invoke(Vector2.zero);
        ingameInputActions.Player.Aim.performed += ctx => Aim.Invoke(true);
        ingameInputActions.Player.Aim.canceled += ctx => Aim.Invoke(false);

        ingameInputActions.Player.Inventory.performed += ctx => Inventory?.Invoke();

        ingameInputActions.Player.Attack.performed += ctx => Attack?.Invoke(true);
        ingameInputActions.Player.Attack.canceled += ctx => Attack?.Invoke(false);

        ingameInputActions.Player.Reload.performed += ctx => Reload?.Invoke(true);
        ingameInputActions.Player.Reload.canceled += ctx => Reload?.Invoke(false);

        ingameInputActions.Player.Interact.performed += ctx => Interact?.Invoke(true);
        ingameInputActions.Player.Interact.canceled += ctx => Interact?.Invoke(false);

        ingameInputActions.Player.Jump.started += ctx => Jump?.Invoke(true);
        ingameInputActions.Player.Jump.canceled += ctx => Jump?.Invoke(false);

        ingameInputActions.Player.Sneak.performed += ctx => Sneak?.Invoke(true);
        ingameInputActions.Player.Sneak.canceled += ctx => Sneak?.Invoke(false);

        ingameInputActions.Player.CycleItems.performed += ctx =>
        {
            if (ctx.ReadValue<Vector2>().y < 0) Next.Invoke(); else Previous.Invoke();
        };

        ingameInputActions.Player.One.performed += ctx => Equip?.Invoke(0);
        ingameInputActions.Player.Two.performed += ctx => Equip?.Invoke(1);
        ingameInputActions.Player.Three.performed += ctx => Equip?.Invoke(2);

        ingameInputActions.Player.Four.performed += ctx => Stash?.Invoke();
        ingameInputActions.Player.Drop.performed += ctx => Drop?.Invoke();

        ingameInputActions.Player.Build.performed += _ => Build.Invoke();
        ingameInputActions.Player.RotateBuild.performed += _ => Rotate.Invoke();
        ingameInputActions.Player.CancelBuild.performed += _ => Cancel.Invoke();

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


public struct InputContext : INetworkSerializable
{
    public float Horizontal;
    public float Vertical;
    public float mouseHorizontal;
    public float mouseVertical;
    public bool climbing;
    public bool crouching;
    public float crouchAmount;
    public bool grounded;
    public bool submerged;
    public Quaternion transformRotation;
    public Vector3 ladderNormal;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        throw new NotImplementedException();
    }
}