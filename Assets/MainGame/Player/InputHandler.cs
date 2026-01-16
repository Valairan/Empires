using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : NetworkBehaviour
{
    private InGame ingameInputActions;
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool Attack { get; private set; }
    public bool Interact { get; private set; }
    public bool Jump { get; private set; }
    public bool Sneak { get; private set; }
    public bool Previous { get; private set; }
    public bool Next { get; private set; }

    void Start()
    {

        ingameInputActions = new InGame();
        ingameInputActions.Player.Move.performed += OnMove;
        ingameInputActions.Player.Move.canceled += OnMove;
        ingameInputActions.Player.Look.performed += OnLook;
        ingameInputActions.Player.Look.canceled += OnLook;
        ingameInputActions.Player.Attack.performed += (ctx) => Attack = true;
        ingameInputActions.Player.Attack.canceled += (ctx) => Attack = false;
        ingameInputActions.Player.Interact.performed += (ctx) => Interact = true;
        ingameInputActions.Player.Interact.canceled += (ctx) => Interact = false;
        ingameInputActions.Player.Jump.performed += (ctx) => Jump = true;
        ingameInputActions.Player.Jump.canceled += (ctx) => Jump = false;
        ingameInputActions.Player.Sneak.performed += (ctx) => Sneak = true;
        ingameInputActions.Player.Sneak.canceled += (ctx) => Sneak = false;
        ingameInputActions.Player.Previous.started += (ctx) => Previous = true;
        ingameInputActions.Player.Previous.canceled += (ctx) => Previous = false;
        ingameInputActions.Player.Next.started += (ctx) => Next = true;
        ingameInputActions.Player.Next.canceled += (ctx) => Next = false;
        ingameInputActions.Enable();
    }
    public override void OnNetworkSpawn()

    {
        if (!IsOwner) return;

        ingameInputActions = new InGame();
        ingameInputActions.Player.Move.performed += OnMove;
        ingameInputActions.Player.Move.canceled += OnMove;
        ingameInputActions.Player.Look.performed += OnLook;
        ingameInputActions.Player.Look.canceled += OnLook;
        ingameInputActions.Player.Attack.performed += (ctx) => Attack = true;
        ingameInputActions.Player.Attack.canceled += (ctx) => Attack = false;
        ingameInputActions.Player.Interact.performed += (ctx) => Interact = true;
        ingameInputActions.Player.Interact.canceled += (ctx) => Interact = false;
        ingameInputActions.Player.Jump.performed += (ctx) => Jump = true;
        ingameInputActions.Player.Jump.canceled += (ctx) => Jump = false;
        ingameInputActions.Player.Sneak.performed += (ctx) => Sneak = true;
        ingameInputActions.Player.Sneak.canceled += (ctx) => Sneak = false;
        ingameInputActions.Player.Previous.started += (ctx) => Previous = true;
        ingameInputActions.Player.Previous.canceled += (ctx) => Previous = false;
        ingameInputActions.Player.Next.started += (ctx) => Next = true;
        ingameInputActions.Player.Next.canceled += (ctx) => Next = false;
        ingameInputActions.Enable();
    }
    public override void OnNetworkDespawn()
    {
        ingameInputActions.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }
    private void OnLook(InputAction.CallbackContext context)
    {
        LookInput = context.ReadValue<Vector2>();
    }


}
