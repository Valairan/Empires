using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;
public class PlayerController : ItemBehaviour, IRaycastResponder, IDamageable
{
    [Header("Components")]
    [SerializeField] public LocomotionController playerCCMotor;
    [SerializeField] public AnimationController playerAnimationController;
    [SerializeField] public CameraController playerCamerasMotor;
    [SerializeField] public CombatController playerCombatController;
    [SerializeField] public InventoryHandler playerInventoryController;
    [SerializeField] public Animator playerAnimator;
    [SerializeField] public SkinnedMeshRenderer playerClothesParent;
    [SerializeField] public InputHandler playerInputHandler;
    [SerializeField] public BuildHandler playerBuildHandler;
    [SerializeField] public InteractionHandler playerInteractionHandler;
    [SerializeField] public Camera playerCamera;
    [SerializeField] public Health health;
    [SerializeField] public Armor armor;

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
        playerCCMotor.motor.SetPosition(GameManager.Singleton.GetSpawnPointForClient(NetworkManager.LocalClientId));
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
        onWeaponChanged += setCurrentAnimatorWeapon;
        onWeaponChanged += playerCombatController.setCurrentWeapon;
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
        playerInputHandler.Aim += OnAim;;

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


    public void Update()
    {
        if (!IsLocalPlayer) return;
        Submerged = checkIfInWater();
        playerBuildHandler.previewBuild(playerCamera.transform.position, playerCamera.transform.forward);
        playerInteractionHandler.checkForRaycasts(playerCamera.transform);
        playerInteractionHandler.HandleTimedInteraction(NetworkManager.Singleton.LocalClientId);
        playerInteractionHandler.interacting = interacting;
        PlayerInputs inputs = new PlayerInputs();
        inputs.Horizontal = MoveInput.x;
        inputs.Vertical = MoveInput.y;
        inputs.transformRotation = playerCamera.transform.rotation;
        playerCCMotor.setInputs(ref inputs);
        Grounded = playerCCMotor.motor.GroundingStatus.IsStableOnGround;
        //playerCCMotor.Move(finalMove * Time.deltaTime);
        updateAnimationParams(MoveInput, Grounded, sidearmAnimation, rifleAnimation, meleeAnimation, Submerged);

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
        playerCamera.transform.position = playerCamerasMotor.HandlePosition(Time.deltaTime, aiming, playerCamera.transform.rotation);

    }
    void OnAim(bool pressed)
    {
        if (!IsLocalPlayer) return;
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
    public void updateAnimationParams(Vector2 movement, bool grounded, bool sideArm, bool rifle, bool melee, bool inwater)
    {
        if (!IsLocalPlayer) return;
        playerAnimator.SetBool("Grounded", grounded);
        playerAnimator.SetBool("Submerged", inwater);
        if (inwater)
        {
            playerAnimator.SetFloat("Horizontal", movement.sqrMagnitude);
            return;
        }
        playerAnimator.SetFloat("Horizontal", Mathf.Round(movement.x));
        playerAnimator.SetFloat("Vertical", Mathf.Round(movement.y));
        playerAnimator.SetBool("SideArm", sideArm);
        playerAnimator.SetBool("Rifle", rifle);
        playerAnimator.SetBool("Melee", melee);


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

    public void Attack(bool input)
    {
        if (input && !Submerged)
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

    public bool checkIfInWater()
    {
        Collider[] cols = Physics.OverlapSphere(GroundCheck.position, 0.02f, WhatIsWater);
        if (cols.Length > 0) return true;
        return false;
    }


}
