using System;
using Unity.Netcode;
using Unity.VisualScripting;
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
        //playerCamerasMotor.ca = playerCamera;
        //playerCamerasMotor.Init();
        playerCombatController.cameraTransform = playerCamera.transform;
        playerCCMotor.init();
        playerAnimationController.init();
        playerInventoryController.init();
        playerCombatController.init();
        BindUI();
        BindInputs();

        BindComponents();
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
        UiController.Singleton.currentPlayerInventoryHandler = playerInventoryController;

        onHealthChanged += UiController.Singleton != null ? UiController.Singleton.setHealth : null;
        onWeaponChanged += UiController.Singleton.weaponChanged;
        playerInventoryController.currentWeaponIndex.OnValueChanged += UiController.Singleton.updateInventoryDisplay;
        //playerInventoryController.weaponStorage.CollectionChanged += UiController.Singleton.updateInventoryDisplay;
        UiController.Singleton.init();

    }

    public void BindComponents()
    {
        playerInventoryController.currentWeaponIndex.OnValueChanged += OnWeaponUpdated;
        playerBuildHandler.locationValidityChange += UiController.Singleton.buildableLocationValid;
    }

    public void BindInputs()
    {

        playerInteractionHandler.init();

        playerInputHandler.Move += ctx => MoveInput = ctx;
        playerInputHandler.Look += ctx => LookInput = ctx;
        playerInputHandler.Attack += Attack;
        playerInputHandler.Sneak += ctx => sneaking = ctx;
        playerInputHandler.Previous += EquipPreviousItem;
        playerInputHandler.Next += EquipNextItem;
        playerInputHandler.Jump += jump;

        playerInputHandler.Inventory += UiController.Singleton.toggleInventory;

        playerInputHandler.Equip += EquipSpecificItem;
        playerInputHandler.Stash += storeCurrentWeapon;
        playerInputHandler.Drop += dropCurrentWeapon;
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
        playerInteractionHandler.checkForRaycasts(OwnerClientId, playerCamera.transform);
        playerInteractionHandler.HandleTimedInteraction(NetworkManager.Singleton.LocalClientId);
        playerInteractionHandler.interacting = interacting;
        PlayerInputs inputs = new PlayerInputs
        {
            Horizontal = MoveInput.x,
            Vertical = MoveInput.y,
            mouseHorizontal = LookInput.x,
            mouseVertical = LookInput.y,
            transformRotation = playerCamera.transform.rotation
        };
        playerCCMotor.setInputs(ref inputs);
        Grounded = playerCCMotor.motor.GroundingStatus.IsStableOnGround;


        currentlyLookingAtPoint = playerCombatController.RaycastFromCamera();
        playerCombatController.UpdateWeapon();
        lookTargetTransform.position = currentlyLookingAtPoint;
        //playerCamerasMotor.Tick(LookInput, Time.deltaTime);
        //playerCCMotor.Move(finalMove * Time.deltaTime);
        playerAnimationController.updateMovemementParams(MoveInput.normalized, Grounded, Submerged);

    }

    public void LateUpdate()
    {
        if (!IsLocalPlayer) return;

        playerCamera.transform.rotation = playerCamerasMotor.HandleRotation(playerCamera.transform.rotation, Time.deltaTime, LookInput);
        playerCamera.transform.position = playerCamerasMotor.HandlePosition(Time.deltaTime, aiming, playerCamera.transform.rotation, playerCamera.transform.position);

        // Update the camera using the new staged system
        //playerCamerasMotor.LateTick();

        // Continue updating animations
        playerAnimationController.LateTick();
    }
    void OnAim(bool pressed)
    {
        if (!IsLocalPlayer) return;
        if (playerCombatController.currentWeapon == null) return;
        if (!playerCombatController.currentWeapon.baseitem.canADS) return;
        if (playerCombatController.currentWeapon.baseitem.WeaponType == WeaponType.melee || playerCombatController.currentWeapon.baseitem.WeaponType == WeaponType.throwable) return;
        if (pressed)
        {
            playerCamera.fieldOfView = 60 * ((RangedWeapon)playerCombatController.currentWeapon.baseitem).scopeZoom;
            foreach (SkinnedMeshRenderer mesh in playerMeshes)
            {
                mesh.enabled = false;
            }
            UiController.Singleton.onAim(pressed);
            //playerInventoryController.currentGO.transform = camera
        }
        else
        {
            playerCamera.fieldOfView = 60;
            foreach (SkinnedMeshRenderer mesh in playerMeshes)
            {
                mesh.enabled = true;
            }
            UiController.Singleton.onAim(pressed);
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
        {
            playerCombatController.OnAttackUp();
            playerCCMotor.jump();
        }
    }

    RecoilStage currentWeaponRecoilStage;

    public void OnWeaponUpdated(int previousValue, int newValue)
    {
        OnWeaponUpdated(newValue);
    }

    public void OnWeaponUpdated(int currentlyEquipped)
    {

        if (currentlyEquipped == -1) // No weapon equipped
        {
            if (playerCombatController.currentWeapon != null)
                if (playerCombatController.currentWeapon.onAttack != null)
                    playerCombatController.currentWeapon.onAttack -= playerAnimationController.attack;

            playerCombatController.currentWeapon = null;
            playerAnimationController.transition(
                playerAnimationController.availableStates[states.Unarmed]);
            return;
        }

        WeaponStorageSlot slot = playerInventoryController.weaponStorage[currentlyEquipped];
        WeaponBehaviour weapon = slot.onplayer_behaviour;
        playerAnimationController.updateCurrentWeapon(weapon);
        playerCombatController.currentWeapon = weapon;

        switch (slot.weapon.WeaponType)
        {
            case WeaponType.rifle:
                playerAnimationController.transition(
                    playerAnimationController.availableStates[states.Rifle]);
                break;

            case WeaponType.sidearm:
                playerAnimationController.transition(
                    playerAnimationController.availableStates[states.Sidearm]);
                break;

            case WeaponType.melee:
                playerAnimationController.transition(
                    playerAnimationController.availableStates[states.Melee]);
                break;

            case WeaponType.throwable:
                playerAnimationController.transition(
                    playerAnimationController.availableStates[states.Throwable]);
                break;
            case WeaponType.overtheshoulder:
                playerAnimationController.transition(
                    playerAnimationController.availableStates[states.OverTheShoulder]);
                break;
        }

        playerCombatController.currentWeapon.onAttack += playerAnimationController.attack;

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

    public void EquipSpecificItem(int index)
    {
        playerInventoryController.EquipWeapon_ServerRpc(index);
    }
    public void EquipNextItem()
    {
        playerInventoryController.NextWeapon_ServerRpc();
    }
    public void EquipPreviousItem()
    {
        playerInventoryController.PreviousWeapon_ServerRpc();
    }
    public void storeCurrentWeapon()
    {
        playerInventoryController.StashCurrentWeapon_ServerRpc();
    }
    public void dropCurrentWeapon()
    {
        playerInventoryController.DropCurrentWeapon_ServerRpc();
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

    public Item respondToRaycast(ulong interactor)
    {
        throw new NotImplementedException();
    }
}

