using System;
using TMPro;
using Unity.Burst.Intrinsics;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : ItemBehaviour, IRaycastResponder, IDamageable
{
    [Header("Components")]
    [SerializeField] CharacterController playerCCMotor;
    [SerializeField] Animator playerAnimator;
    [SerializeField] SkinnedMeshRenderer playerClothesParent;
    [SerializeField] InputHandler playerInputHandler;
    [SerializeField] Health health;
    [SerializeField] Armor armor;

    [Header("Locomotion Settings")]
    bool Grounded;
    [SerializeField] float runSpeed;
    [SerializeField] float walkSpeed;
    [SerializeField] Transform GroundCheck;
    [SerializeField] LayerMask WhatIsGround;
    Vector3 MoveDirection;

    [Header("Camera Settings")]
    [SerializeField] float sensitivity = 150f;
    [SerializeField] float minPitch = -80f;
    [SerializeField] float maxPitch = 80f;
    float yaw;
    float pitch;
    [SerializeField] Camera playerCamera;
    [SerializeField] Transform playerCameraParent;
    [SerializeField] Transform playerCameraOrbit;

    public Item current;
    public Item primary;
    public Item sidearm;
    public Item melee;
    public GameObject meleeGO;
    public Action<float> onHealthChanged;
    public Action<float> onArmorChanged;
    public Action<float> onInteractionProgressChanged;
    public Action<bool> onInteractableInView;
    public Action<Item> onLookingAtChanged;
    public Item currentlyLookingAt;
    ulong clientID;

    public void setClientId(ulong clientID)
    {
        this.clientID = clientID;
    }

    public void wearItem(Wearable item)
    {

    }
    public override void OnNetworkSpawn()
    {
        if (!IsLocalPlayer) return;
        onHealthChanged += UiController.Singleton != null ? UiController.Singleton.setHealth : null;
        onLookingAtChanged += UiController.Singleton != null ? UiController.Singleton.setCurerntlyLookingAt : null;
        onInteractionProgressChanged += UiController.Singleton != null ? UiController.Singleton.setInteractionProgress : null;
        onInteractableInView += UiController.Singleton != null ? UiController.Singleton.displayInteractIcon : null;
        playerCamera = Camera.main;
    }
    public void Update()
    {
        if (!IsLocalPlayer) return;
        MoveDirection = new Vector3(playerInputHandler.MoveInput.normalized.x * (playerInputHandler.Sneak ? walkSpeed : runSpeed), 0, playerInputHandler.MoveInput.normalized.y * (playerInputHandler.Sneak ? walkSpeed : runSpeed));
        playerCCMotor.Move(MoveDirection * Time.deltaTime);
        Collider[] groundColliders = Physics.OverlapSphere(GroundCheck.position, .2f, WhatIsGround);
        if (groundColliders.Length > 0)
            Grounded = true;
        else
            Grounded = false;

        checkForRaycasts();
        updateAnimationParams(playerInputHandler.MoveInput, false, false, false, false);
    }

    public void LateUpdate()
    {
        if (!IsLocalPlayer) return;
        playerCamera.transform.position = playerCameraParent.position;
        playerCamera.transform.rotation = playerCameraParent.rotation;
        yaw += playerInputHandler.LookInput.x * sensitivity * Time.deltaTime;
        pitch -= playerInputHandler.LookInput.y * sensitivity * Time.deltaTime; // inverted Y

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        playerCameraOrbit.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    public void updateAnimationParams(Vector2 movement, bool grounded, bool sideArm, bool rifle, bool melee)
    {
        playerAnimator.SetFloat("Horizontal", movement.x);
        playerAnimator.SetFloat("Vertical", movement.y);
    }

    public void checkForRaycasts()
    {
        if (Physics.SphereCast(playerCamera.transform.position, 0.5f, playerCamera.transform.forward, out RaycastHit hit, 25f))
        {
            if (hit.transform.TryGetComponent(out IRaycastResponder responder))
            {
                Item item = responder.respondToRaycast();
                if (item != currentlyLookingAt)
                {
                    currentlyLookingAt = item;
                    onLookingAtChanged?.Invoke(item);
                }
                if (hit.transform.TryGetComponent(out IInteractable interactable))
                    onInteractableInView.Invoke(true);
                else
                    onInteractableInView.Invoke(false);
            }

        }
    }

    public Item respondToRaycast()
    {
        return baseitem;
    }

    public void takeDamage(float damage)
    {
        armor.amount -= damage;
        health.amount = armor.amount < 0 ? health.amount + armor.amount : health.amount;
        armor.amount = armor.amount < 0 ? 0 : armor.amount;
        onArmorChanged.Invoke(armor.amount);
        onHealthChanged.Invoke(health.amount);

    }
    void attemptToDamage()
    {

        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, 25f))
        {

        }
    }
    [ServerRpc]
    void attemptToDamage_ServerRpc()
    {

    }
}
