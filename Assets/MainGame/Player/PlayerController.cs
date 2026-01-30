using System;
using KinematicCharacterController;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class PlayerController : ItemBehaviour, IRaycastResponder, IDamageable
{
    [Header("Components")]
    [SerializeField] LocomotionController playerCCMotor;
    [SerializeField] CameraController playerCamerasMotor;
    [SerializeField] CombatController playerCombatController;
    [SerializeField] InventoryHandler playerInventoryController;
    [SerializeField] Animator playerAnimator;
    [SerializeField] SkinnedMeshRenderer playerClothesParent;
    [SerializeField] InputHandler playerInputHandler;
    [SerializeField] BuildHandler playerBuildHandler;
    [SerializeField] InteractionHandler playerInteractionHandler;
    [SerializeField] Camera playerCamera;
    [SerializeField] Health health;
    [SerializeField] Armor armor;

    [Header("Locomotion Settings")]
    bool Grounded = true;
    bool Submerged = false;

    [SerializeField] float runSpeed;
    [SerializeField] float walkSpeed;
    [SerializeField] float gravity;
    [SerializeField] float jumpHeight;
    [SerializeField]
    float turnSmoothVelocity;
    public float turnSmoothTime = 0.1f;
    [SerializeField] Transform GroundCheck;
    [SerializeField] LayerMask WhatIsGround;
    Vector3 velocity;


    public Action<float> onHealthChanged;
    public Action<float> onArmorChanged;

    public ulong clientID;

    public Vector2 MoveInput;
    public Vector2 LookInput;
    public bool aiming;
    public bool sneaking;
    public bool interacting;

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
        playerCamera = Camera.main;
        BindUI();
        BindInputs();
        playerCamerasMotor.setFollowTransform(playerCamerasMotor.cameraFollowTarget);
        playerCCMotor.init();
        playerInventoryController.init();

    }

    public void BindUI()
    {
        onHealthChanged += UiController.Singleton != null ? UiController.Singleton.setHealth : null;


        UiController.Singleton.currentPlayerBuildHandler = playerBuildHandler;
    }

    public void BindInputs()
    {

        playerInteractionHandler.Init();

        playerInputHandler.Move += ctx => MoveInput = ctx;
        playerInputHandler.Look += ctx => LookInput = ctx;
        playerInputHandler.Attack += Attack;
        playerInputHandler.Sneak += ctx => sneaking = ctx;
        playerInputHandler.Previous += equipPreviousItem;
        playerInputHandler.Next += equipNextItem;
        playerInputHandler.Jump += jump;
        playerInputHandler.Interact += OnInteract;
        playerInputHandler.Aim += OnAim;

        playerInputHandler.Build += buildButtonPressed;
        playerInputHandler.Rotate += playerBuildHandler.rotateButtonPressed;
        playerInputHandler.Cancel += playerBuildHandler.CancelButtonPressed;

        playerInputHandler.enableInputs();
    }

    Vector3 groundPoint;
    public override void OnNetworkDespawn()
    {
        playerInputHandler.disableInputs();
    }
    Vector3 horizontalVelocity;


    public void Update()
    {
        if (!IsLocalPlayer) return;

        playerBuildHandler.previewBuild(playerCamera.transform.position, playerCamera.transform.forward);
        playerInteractionHandler.checkForRaycasts(playerCamera.transform);
        playerInteractionHandler.HandleTimedInteraction();
        playerInteractionHandler.interacting = interacting;
        PlayerInputs inputs = new PlayerInputs();
        inputs.Horizontal = MoveInput.x;
        inputs.Vertical = MoveInput.y;
        inputs.transformRotation = playerCamera.transform.rotation;
        playerCCMotor.setInputs(ref inputs);
        Grounded = playerCCMotor.motor.GroundingStatus.IsStableOnGround;
        //playerCCMotor.Move(finalMove * Time.deltaTime);
        updateAnimationParams(MoveInput, Grounded, false, false, false, Submerged);

    }

    public void LateUpdate()
    {
        if (!IsLocalPlayer) return;

        playerCamera.transform.rotation = playerCamerasMotor.HandleRotation(playerCamera.transform.rotation, Time.deltaTime, LookInput);
        playerCamera.transform.position = playerCamerasMotor.HandlePosition(Time.deltaTime, aiming, playerCamera.transform.rotation);

    }
    void OnAim(bool pressed)
    {
        aiming = pressed;


    }
    void OnInteract(bool pressed)
    {
        interacting = pressed;

        if (!pressed)
        {
            playerInteractionHandler.interactTimer = 0f;
            playerInteractionHandler.onInteractionProgressChanged?.Invoke(0f);
        }
    }
    public void jump(bool input)
    {
        if (input)
            playerCCMotor.jump();
    }
    public void updateAnimationParams(Vector2 movement, bool grounded, bool sideArm, bool rifle, bool melee, bool inwater)
    {
        if (inwater && (movement.magnitude != 0))
        {
            playerAnimator.SetBool("Submerged", inwater);
            playerAnimator.SetFloat("Horizontal", 1);
            return;
        }
        playerAnimator.SetFloat("Horizontal", Mathf.Round(movement.x));
        playerAnimator.SetFloat("Vertical", Mathf.Round(movement.y));
        playerAnimator.SetBool("Grounded", grounded);

    }

    public void buildButtonPressed()
    {
        playerBuildHandler.buildButtonPressed();
    }

    public Item respondToRaycast()
    {
        return baseitem;
    }

    public void equipNextItem()
    {

    }
    public void equipPreviousItem()
    {

    }

    public void Attack(bool input)
    {
        if (input)
        {
            playerCombatController.Attack();
            playerAnimator.SetTrigger("Attack");
        }

    }

    public void takeDamage(Weapon damager)
    {

        //armor.amount -= damage;
        //health.amount = armor.amount < 0 ? health.amount + armor.amount : health.amount;
        ///armor.amount = armor.amount < 0 ? 0 : armor.amount;
        //onArmorChanged.Invoke(armor.amount);
        onHealthChanged.Invoke(health.amount);
    }
    void attemptToDamage()
    {
        if (!IsLocalPlayer) return;
        attemptToDamage_ServerRpc();
    }
    [ServerRpc]
    void attemptToDamage_ServerRpc()
    {

    }


    public bool PickUpItem(ItemBehaviour itemGOS)
    {
        playerInventoryController.PickUpItem_ServerRpc(itemGOS.NetworkObjectId, clientID);
        return true;
    }


}
