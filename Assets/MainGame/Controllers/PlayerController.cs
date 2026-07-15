using System;
using System.Reflection;
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
    public Vector2 MoveInput;
    Vector2 smoothedMoveInput;
    [SerializeField] float movementInputAcceleration = 10f;
    [SerializeField] float movementInputDeceleration = 15f;
    [SerializeField] float interpolationFactor;
    public Vector2 LookInput;
    public bool aiming;
    public bool sneaking;
    public bool interacting;
    bool Grounded = true;
    bool Crouching;
    bool Climbing;
    bool Submerged
    {
        get { return submerged; }
        set
        {
            if (value && !submerged) onEnterAndExitWater?.Invoke(value);

            submerged = value;
        }
    }
    bool submerged;
    public Action<bool> groundedForVfx;
    public Action<bool> onEnterAndExitWater;


    [SerializeField]
    float turnSmoothVelocity;
    public float turnSmoothTime = 0.1f;
    [SerializeField] Transform lookTargetTransform;
    [SerializeField] Transform GroundCheck;
    [SerializeField] LayerMask WhatIsGround;
    [SerializeField] LayerMask WhatIsWater;
    [SerializeField] LayerMask WhatIsClimbable;
    [Header("Camera States")]
    [SerializeField] Vector3 defaultCameraPosition;
    [SerializeField] Vector3 crouchCameraPosition;
    [SerializeField] Vector3 climbCameraPosition;
    [SerializeField] float cameraStateInterpolationSpeed;
    Vector3 velocity;

    public Action<Weapon> onWeaponChanged;


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
        BindVfx();
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
    #region UI Events
    public void BindUI()
    {
        UiController.Singleton.currentPlayerBuildHandler = playerBuildHandler;
        UiController.Singleton.currentPlayerInventoryHandler = playerInventoryController;

        onWeaponChanged += UiController.Singleton.weaponChanged;
        playerInventoryController.currentWeaponIndex.OnValueChanged += UiController.Singleton.updateInventoryDisplay;
        //playerInventoryController.weaponStorage.CollectionChanged += UiController.Singleton.updateInventoryDisplay;

        health.currentAmount.OnValueChanged += onHealthChanged;
        armor.currentAmount.OnValueChanged += onArmorChanged;

        UiController.Singleton.init();

    }
    public void DetachUI()
    {

        onWeaponChanged -= UiController.Singleton.weaponChanged;
        playerInventoryController.currentWeaponIndex.OnValueChanged -= UiController.Singleton.updateInventoryDisplay;
        //playerInventoryController.weaponStorage.CollectionChanged -= UiController.Singleton.updateInventoryDisplay;

        health.currentAmount.OnValueChanged -= onHealthChanged;
        armor.currentAmount.OnValueChanged -= onArmorChanged;
    }
    #endregion
    #region Component Events
    public void BindComponents()
    {
        playerInventoryController.currentWeaponIndex.OnValueChanged += OnWeaponUpdated;
        playerBuildHandler.locationValidityChange += UiController.Singleton.buildableLocationValid;
    }
    public void DetachComponents()
    {
        playerInventoryController.currentWeaponIndex.OnValueChanged -= OnWeaponUpdated;
        playerBuildHandler.locationValidityChange -= UiController.Singleton.buildableLocationValid;
    }
    #endregion
    #region Input Events
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
        playerInputHandler.Sneak += crouch;

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
    public void DetachInputs()
    {

        playerInteractionHandler.init();

        playerInputHandler.Move -= ctx => MoveInput = ctx;
        playerInputHandler.Look -= ctx => LookInput = ctx;
        playerInputHandler.Attack -= Attack;
        playerInputHandler.Sneak -= ctx => sneaking = ctx;
        playerInputHandler.Previous -= EquipPreviousItem;
        playerInputHandler.Next -= EquipNextItem;
        playerInputHandler.Jump -= jump;
        playerInputHandler.Sneak -= crouch;

        playerInputHandler.Inventory -= UiController.Singleton.toggleInventory;

        playerInputHandler.Equip -= EquipSpecificItem;
        playerInputHandler.Stash -= storeCurrentWeapon;
        playerInputHandler.Drop -= dropCurrentWeapon;
        playerInputHandler.Interact -= OnInteract;
        playerInputHandler.Aim -= OnAim;

        playerInputHandler.Build -= buildButtonPressed;
        playerInputHandler.Rotate -= playerBuildHandler.rotateButtonPressed;
        playerInputHandler.Cancel -= playerBuildHandler.CancelButtonPressed;

        playerInputHandler.disableInputs();
    }

    InputContext currentInputs = new InputContext
    {
        Horizontal = 0f,
        Vertical = 0f,
        climbing = false,
        crouching = false,
        mouseHorizontal = 0f,
        mouseVertical = 0f,
        transformRotation = Quaternion.identity,
        ladderNormal = Vector3.zero
    };
    Vector2 InterpolateMovementInput(Vector2 target)
    {
        float rate = movementInputAcceleration;

        smoothedMoveInput = Vector2.MoveTowards(
            smoothedMoveInput,
            target,
            rate * Time.deltaTime
        );
        return smoothedMoveInput;
    }

    #endregion 

    public override void OnNetworkDespawn()
    {
        if (!IsLocalPlayer) return;
        DetachComponents();
        DetachInputs();
        DetachUI();

    }

    public void BindVfx()
    {
        //onEnterAndExitWater += VisualsManager.Singleton.RequestPlayEffect_ServerRpc();
    }

    Vector3 currentlyLookingAtPoint;
    public void Update()
    {
        if (!IsLocalPlayer) return;
        Submerged = checkIfInWater();
        Climbing = checkIfInLadder();
        playerBuildHandler.previewBuild(playerCamera.transform.position, playerCamera.transform.forward);
        playerInteractionHandler.checkForRaycasts(OwnerClientId, playerCamera.transform);
        playerInteractionHandler.HandleTimedInteraction(NetworkManager.Singleton.LocalClientId);
        playerInteractionHandler.interacting = interacting;

        Vector2 smoothMovement = InterpolateMovementInput(MoveInput);

        currentInputs = new InputContext
        {
            Horizontal = smoothMovement.x,
            Vertical = smoothMovement.y,
            climbing = Climbing,
            crouching = Crouching,
            mouseHorizontal = LookInput.x,
            mouseVertical = LookInput.y,
            transformRotation = playerCamera.transform.rotation,
            ladderNormal = currentLadderNormal
        };
        playerCCMotor.setInputs(ref currentInputs);
        Grounded = playerCCMotor.motor.GroundingStatus.IsStableOnGround;

        currentlyLookingAtPoint = playerCombatController.RaycastFromCamera();
        playerCombatController.UpdateWeapon();
        lookTargetTransform.position = currentlyLookingAtPoint;
        //playerCamerasMotor.Tick(LookInput, Time.deltaTime);
        //playerCCMotor.Move(finalMove * Time.deltaTime);
        playerAnimationController.updateMovemementParams(MoveInput.normalized, Grounded, Climbing, Submerged, !Crouching ? 1 : 0);

        playerAnimationController.Tick();
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
        UpdateCameraFollowPosition();
    }

    Vector3 cameraShouldBeHere = Vector3.zero;
    public void UpdateCameraFollowPosition()
    {
        if (Climbing)
        {
            playerCamerasMotor.cameraFollowTarget.localPosition = Vector3.MoveTowards(playerCamerasMotor.cameraFollowTarget.localPosition, climbCameraPosition, cameraStateInterpolationSpeed * Time.deltaTime);
            return;
        }
        else if (Crouching)
        {
            playerCamerasMotor.cameraFollowTarget.localPosition = Vector3.MoveTowards(playerCamerasMotor.cameraFollowTarget.localPosition, crouchCameraPosition, cameraStateInterpolationSpeed * Time.deltaTime);
            return;
        }
        playerCamerasMotor.cameraFollowTarget.localPosition = Vector3.MoveTowards(playerCamerasMotor.cameraFollowTarget.localPosition, defaultCameraPosition, cameraStateInterpolationSpeed * Time.deltaTime);
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

    public void crouch(bool input)
    {
        if (!IsLocalPlayer) return;

        Crouching = input;
    }

    RecoilStage currentWeaponRecoilStage;

    public void OnWeaponUpdated(int previousValue, int newValue)
    {

        if (newValue == -1) // No weapon equipped
        {
            if (playerCombatController.currentWeapon != null)
                if (playerCombatController.currentWeapon.onAttack != null)
                    playerCombatController.currentWeapon.onAttack -= playerAnimationController.attack;

            playerCombatController.currentWeapon = null;
            playerAnimationController.transition(
                playerAnimationController.availableStates[states.Unarmed]);
            return;
        }

        WeaponStorageSlot slot = playerInventoryController.weaponStorage[newValue];
        WeaponBehaviour weapon = slot.onplayer_behaviour;
        playerAnimationController.updateCurrentWeapon(weapon);
        playerCombatController.currentWeapon = weapon;

        playerAnimationController.transition(playerAnimationController.availableStates[states.EquipState]);

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
        Debug.Log("take damage is called from: " + OwnerClientId);
        DamagePlayerOn_ServerRpc(ctx);
    }

    [ServerRpc(RequireOwnership = false)]
    //called from take damage
    public void DamagePlayerOn_ServerRpc(DamageContext ctx, ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;
        Debug.Log("rpc is called by: " + rpcParams.Receive.SenderClientId);
        health.decreaseAmount(ctx.damage);

    }


    void onArmorChanged(float previous, float current)
    {
        //UiController.Singleton.(current);
    }
    void onHealthChanged(float previous, float current)
    {
        UiController.Singleton.setHealth(current);
    }


    public bool checkIfInWater()
    {
        Collider[] cols = Physics.OverlapSphere(GroundCheck.position, 0.02f, WhatIsWater);
        if (cols.Length > 0) return true;
        return false;
    }

    [SerializeField] Transform ladderCheck;
    [SerializeField] Vector3 LadderDetectionBoxSize = new Vector3(0.2f, .2f, 0.2f);
    private Vector3 currentLadderNormal = Vector3.up;

    public bool checkIfInLadder()
    {
        Collider[] cols = Physics.OverlapBox(ladderCheck.position, LadderDetectionBoxSize, transform.rotation, WhatIsClimbable);
        if (cols.Length > 0)
        {
            Debug.DrawRay(transform.position, (cols[0].transform.position - transform.position) * 2f, Color.red);
            if (Physics.SphereCast(transform.position, 0.1f, transform.forward, out RaycastHit hit, 0.6f, WhatIsClimbable))
            {
                Debug.DrawRay(hit.point, hit.normal * 2f, Color.blue);
                currentLadderNormal = hit.normal;
                return true;
            }
        }
        else
        {
            return false;
        }
        return false;
    }

    public Item respondToRaycast(ulong interactor)
    {
        throw new NotImplementedException();
    }

    public void PlayVfx()
    {

    }

    void OnDrawGizmos()
    {
        Gizmos.DrawCube(ladderCheck.position, LadderDetectionBoxSize * 2);
    }

}

