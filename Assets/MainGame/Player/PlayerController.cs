using System;
using KinematicCharacterController;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : ItemBehaviour, IRaycastResponder, IDamageable
{
    [Header("Components")]
    [SerializeField] LocomotionController playerCCMotor;
    [SerializeField] CameraController playerCamerasMotor;
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

    [Header("Weapons Settings")]
    public Item current;
    public Item primary;
    public Item sidearm;
    public Item melee;
    public WeaponBehaviour currentBehaviour;
    public WeaponBehaviour primaryBehaviour;
    public WeaponBehaviour sidearmBehaviour;
    public WeaponBehaviour meleeBehaviour;
    [SerializeField] public Transform equipped;
    [SerializeField] public Transform meleeStorage;
    [SerializeField] public Transform primaryStorage;
    [SerializeField] public Transform sideArmStorage;
    [HideInInspector] public GameObject currentGO;
    [HideInInspector] public GameObject primaryGO;
    [HideInInspector] public GameObject sidearamGO;
    [HideInInspector] public GameObject meleeGO;

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

    public void Attack()
    {

    }


    public void takeDamage(Weapon damager)
    {

        //armor.amount -= damage;
        //health.amount = armor.amount < 0 ? health.amount + armor.amount : health.amount;
        ///armor.amount = armor.amount < 0 ? 0 : armor.amount;
        onArmorChanged.Invoke(armor.amount);
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

    public void EquipItem(Item item)
    {
        if (!IsLocalPlayer) return;
        //EquipItem_ServerRpc(item);
    }

    [ServerRpc]
    public void EquipItem_ServerRpc()
    {


    }
    public bool PickUpItem(ItemBehaviour itemGO)
    {
        Item item = itemGO.baseitem;

        if (!IsLocalPlayer) return false;
        switch (item.Type)
        {
            case ItemType.melee:
                melee = item;
                itemGO.NetworkObject.ChangeOwnership(clientID);
                if (itemGO.NetworkObject.TrySetParent(meleeStorage.transform, false))
                {
                    Debug.Log("melee pickup");
                    return false;
                }
                break;
            case ItemType.primary:
                primary = item;
                itemGO.NetworkObject.ChangeOwnership(clientID);
                if (itemGO.NetworkObject.TrySetParent(primaryStorage.transform, false))
                {
                    return false;
                }
                break;
            case ItemType.sidearm:
                sidearm = item;
                itemGO.NetworkObject.ChangeOwnership(clientID);
                if (!itemGO.NetworkObject.TrySetParent(sideArmStorage.transform, false))
                {
                    return false;
                }
                break;
            case ItemType.resource:
                break;
            default:
                break;
        }

        itemGO.transform.localPosition = item.position;
        itemGO.transform.localEulerAngles = item.rotation;
        itemGO.transform.localScale = item.scale;
        current = item;

        return true;
    }

    [ServerRpc]
    public void PickUpItem_ServerRpc()
    {

    }

}
