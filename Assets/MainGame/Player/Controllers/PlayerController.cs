using System;
using Unity.Netcode;
using UnityEngine;
public class PlayerController : ItemBehaviour<Item>, IRaycastResponder, IDamageable
{
    [Header("Components")]
    [SerializeField] public LocomotionController playerCCMotor;
    [SerializeField] public AnimationController playerAnimationController;
    [SerializeField] public CameraController playerCamerasMotor;
    [SerializeField] public CombatController playerCombatController;
    [SerializeField] public InventoryHandler playerInventoryController;
    [SerializeField] public SkinnedMeshRenderer playerClothesParent;
    [SerializeField] public InputHandler playerInputHandler;
    [SerializeField] public BuildHandler playerBuildHandler;
    [SerializeField] public InteractionHandler playerInteractionHandler;
    [SerializeField] public Camera playerCamera;
    [SerializeField] public Health health;
    [SerializeField] public Armor armor;

    [SerializeField] SkinnedMeshRenderer[] playerMeshes;


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
    [SerializeField] Transform lookTargetTransform;
    [SerializeField] Transform GroundCheck;
    [SerializeField] LayerMask WhatIsGround;
    [SerializeField] LayerMask WhatIsWater;
    Vector3 velocity;


    public Action<float> onHealthChanged;
    public Action<float> onArmorChanged;
    public Action<Weapon> onWeaponChanged;


    public Vector2 MoveInput;
    public Vector2 LookInput;
    public bool aiming;
    public bool sneaking;
    public bool interacting;



    public override void OnNetworkSpawn()
    {
        //playerCCMotor.motor.SetPosition(GameManager.Singleton.GetSpawnPointForClient(NetworkManager.LocalClientId));
        if (!IsLocalPlayer)
        {
            disableComponents();
            return;
        }

        enableComponents();
        UiController.Singleton.toggleInGameUI();

        playerCamera = Camera.main;
        BindUI();
        BindInputs();
        playerCamerasMotor.setFollowTransform(playerCamerasMotor.cameraFollowTarget);
        playerCCMotor.init();
        playerInventoryController.init();
        playerCombatController.init();
        onWeaponChanged += setCurrentAnimatorWeapon;
        base.OnNetworkDespawn();
    }

    public void disableComponents()
    {
        playerCCMotor.motor.enabled = false;
        playerCCMotor.enabled = false;
        playerInteractionHandler.enabled = false;
        playerInventoryController.enabled = false;
        playerCombatController.enabled = false;
        playerBuildHandler.enabled = false;
        playerCamerasMotor.enabled = false;
        health.enabled = false;
    }

    public void enableComponents()
    {
        playerCCMotor.motor.enabled = true;
        playerCCMotor.enabled = true;
        playerInteractionHandler.enabled = true;
        playerInventoryController.enabled = true;
        playerCombatController.enabled = true;
        playerBuildHandler.enabled = true;
        playerCamerasMotor.enabled = true;
        health.enabled = true;
    }

    public void BindUI()
    {
        UiController.Singleton.currentPlayerBuildHandler = playerBuildHandler;
        onHealthChanged += UiController.Singleton != null ? UiController.Singleton.setHealth : null;
        onWeaponChanged += UiController.Singleton.weaponChanged;
        playerInputHandler.Aim += UiController.Singleton.onAim;
        UiController.Singleton.init();

    }

    public void BindInputs()
    {

        playerInteractionHandler.init();

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

    public override void OnNetworkDespawn()
    {
        if (!IsLocalPlayer) return;

        playerInputHandler.disableInputs();
    }


    Vector3 currentlyLookingAtPoint;
    public void Update()
    {
        if (!IsLocalPlayer) return;
        Submerged = checkIfInWater();
        playerBuildHandler.previewBuild(playerCamera.transform.position, playerCamera.transform.forward);
        playerInteractionHandler.checkForRaycasts(playerCamera.transform);
        playerInteractionHandler.HandleTimedInteraction(NetworkManager.Singleton.LocalClientId);
        playerInteractionHandler.interacting = interacting;
        PlayerInputs inputs = new PlayerInputs
        {
            Horizontal = MoveInput.x,
            Vertical = MoveInput.y,
            transformRotation = playerCamera.transform.rotation
        };
        playerCCMotor.setInputs(ref inputs);
        Grounded = playerCCMotor.motor.GroundingStatus.IsStableOnGround;

        currentlyLookingAtPoint = playerCombatController.RaycastFromCamera();
        playerCombatController.UpdateWeapon();
        lookTargetTransform.position = currentlyLookingAtPoint;
        //playerCCMotor.Move(finalMove * Time.deltaTime);
        playerAnimationController.updateAnimationParams(MoveInput, Grounded, sidearmAnimation, rifleAnimation, meleeAnimation, Submerged);

    }
    public bool sidearmAnimation = false;
    public bool rifleAnimation = false;
    public bool meleeAnimation = false;

    public void setCurrentAnimatorWeapon(Weapon weapon)
    {
        if (weapon)
            switch (weapon.WeaponType)
            {
                case WeaponType.melee:
                    meleeAnimation = true;
                    sidearmAnimation = false;
                    rifleAnimation = false;
                    break;
                case WeaponType.sidearm:
                    meleeAnimation = false;
                    sidearmAnimation = true;
                    rifleAnimation = false;
                    break;
                case WeaponType.rifle:
                    meleeAnimation = false;
                    sidearmAnimation = false;
                    rifleAnimation = true;
                    break;
            }
    }
    public void LateUpdate()
    {
        if (!IsLocalPlayer) return;

        playerCamera.transform.rotation = playerCamerasMotor.HandleRotation(playerCamera.transform.rotation, Time.deltaTime, LookInput);
        playerCamera.transform.position = playerCamerasMotor.HandlePosition(Time.deltaTime, aiming, playerCamera.transform.rotation, playerCamera.transform.position);

    }
    void OnAim(bool pressed)
    {
        if (!IsLocalPlayer) return;
        if (playerInventoryController.current.WeaponType == WeaponType.melee || playerInventoryController.current.WeaponType == WeaponType.throwable) return;
        if (pressed)
        {
            playerCamera.fieldOfView = 60 * ((RangedWeapon)playerInventoryController.current).scopeZoom;
            foreach (SkinnedMeshRenderer mesh in playerMeshes)
            {
                mesh.enabled = false;
            }
            //playerInventoryController.currentGO.transform = camera
        }
        else
        {
            playerCamera.fieldOfView = 60;
            foreach (SkinnedMeshRenderer mesh in playerMeshes)
            {
                mesh.enabled = true;
            }
        }
        aiming = pressed;


    }
    void OnInteract(bool pressed)
    {
        if (!IsLocalPlayer) return;
        interacting = pressed;

        if (!pressed)
        {
            playerInteractionHandler.interactTimer = 0f;
            playerInteractionHandler.onInteractionProgressChanged?.Invoke(0f);
        }
    }
    public void jump(bool input)
    {
        if (!IsLocalPlayer) return;
        if (input && !Submerged)
            playerCCMotor.jump();
    }



    public void buildButtonPressed()
    {
        if (!IsLocalPlayer) return;
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
    public void dropItem()
    {
        //playerInventoryController.DropWeapon(playerInventoryController.current);
    }

    public void Attack(bool input)
    {
        if (Submerged || !Grounded) return;
        if (input) playerCombatController.OnAttackDown();
        else playerCombatController.OnAttackUp();

    }

    public void takeDamage(DamageContext ctx)
    {

        //armor.amount -= damage;
        //health.amount = armor.amount < 0 ? health.amount + armor.amount : health.amount;
        ///armor.amount = armor.amount < 0 ? 0 : armor.amount;
        //onArmorChanged.Invoke(armor.amount);
        onHealthChanged.Invoke(health.amount);
    }

    public bool checkIfInWater()
    {
        Collider[] cols = Physics.OverlapSphere(GroundCheck.position, 0.02f, WhatIsWater);
        if (cols.Length > 0) return true;
        return false;
    }


}
