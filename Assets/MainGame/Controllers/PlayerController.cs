using System;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;
public class PlayerController : ItemBehaviour<Item>, IRaycastResponder, IDamageable
{
    [Header("Components")]
    [SerializeField] public LocomotionController playerCCMotor;
    [SerializeField] public AnimationController playerAnimationController;
    [SerializeField] public CameraController playerCameraMotor;
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

    [HideInInspector] public bool preventInput = false;
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
        //playerCameraMotor.ca = playerCamera;
        //playerCameraMotor.Init();
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

    public override void OnNetworkPreDespawn()
    {
        DetachUI();
        DetachInputs();
        DetachComponents();
    }

    public void disableComponents()
    {
        playerCCMotor.motor.enabled = false;
        playerCCMotor.enabled = false;
        playerInteractionHandler.enabled = false;
        playerInventoryController.enabled = false;
        playerCombatController.enabled = false;
        playerBuildHandler.enabled = false;
        playerCameraMotor.enabled = false;
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
        playerCameraMotor.enabled = true;
        health.enabled = true;
    }
    #region UI Events
    public void BindUI()
    {
        UiController.Singleton.currentPlayerBuildHandler = playerBuildHandler;
        UiController.Singleton.currentPlayerInventoryHandler = playerInventoryController;

        onWeaponChanged += UiController.Singleton.weaponChanged;
        //playerInventoryController.currentWeaponIndex.OnValueChanged += UiController.Singleton.updateInventoryDisplay;
        //playerInventoryController.weaponStorage.CollectionChanged += UiController.Singleton.updateInventoryDisplay;

        health.currentAmount.OnValueChanged += onHealthChanged;
        armor.currentAmount.OnValueChanged += onArmorChanged;

        UiController.Singleton.init();

    }
    public void DetachUI()
    {

        onWeaponChanged -= UiController.Singleton.weaponChanged;
        //playerInventoryController.currentWeaponIndex.OnValueChanged -= UiController.Singleton.updateInventoryDisplay;
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
    public void toggleInput()
    {
        preventInput = !preventInput;
    }
    public void BindInputs()
    {

        playerInteractionHandler.init();

        playerInputHandler.Move += ctx => MoveInput = ctx;
        playerInputHandler.Look += ctx => LookInput = ctx;
        playerInputHandler.Attack += Attack;
        playerInputHandler.Reload += (bool input) => Debug.Log("Tomato");
        playerInputHandler.Firemode += toggleFiremode;
        playerInputHandler.Sneak += ctx => sneaking = ctx;
        playerInputHandler.Previous += EquipPreviousItem;
        playerInputHandler.Next += EquipNextItem;
        playerInputHandler.Jump += jump;
        playerInputHandler.Sneak += crouch;

        playerInputHandler.Inventory += UiController.Singleton.toggleInventory;
        playerInputHandler.Inventory += toggleInput;

        playerInputHandler.Equip += EquipSpecificItem;
        playerInputHandler.Stash += storeCurrentWeapon;
        playerInputHandler.Drop += dropCurrentWeapon;
        playerInputHandler.Interact += OnInteract;
        playerInputHandler.Aim += OnAim;

        playerInputHandler.Build += buildButtonPressed;
        playerInputHandler.Build += toggleInput;
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
        playerInputHandler.Inventory -= toggleInput;

        playerInputHandler.Equip -= EquipSpecificItem;
        playerInputHandler.Stash -= storeCurrentWeapon;
        playerInputHandler.Drop -= dropCurrentWeapon;
        playerInputHandler.Interact -= OnInteract;
        playerInputHandler.Aim -= OnAim;

        playerInputHandler.Build -= buildButtonPressed;
        playerInputHandler.Build -= toggleInput;

        playerInputHandler.Rotate -= playerBuildHandler.rotateButtonPressed;
        playerInputHandler.Cancel -= playerBuildHandler.CancelButtonPressed;

        playerInputHandler.disableInputs();
    }

    InputContext rawInputs = new InputContext();
    InputContext smoothedInputs = new InputContext();
    InputContext InterpolateInput(InputContext raw)
    {
        InputContext result = raw;

        result.Horizontal = Mathf.MoveTowards(
            smoothedInputs.Horizontal,
            raw.Horizontal,
            interpolationFactor * Time.deltaTime
        );

        result.Vertical = Mathf.MoveTowards(
            smoothedInputs.Vertical,
            raw.Vertical,
            interpolationFactor * Time.deltaTime
        );

        result.crouchAmount = Mathf.MoveTowards(
            smoothedInputs.crouchAmount,
            raw.crouchAmount,
            interpolationFactor * Time.deltaTime
        );

        return result;
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



        smoothedInputs = InterpolateInput(new InputContext
        {
            Horizontal = MoveInput.x,
            Vertical = MoveInput.y,
            mouseHorizontal = LookInput.x,
            mouseVertical = LookInput.y,
            transformRotation = playerCamera.transform.rotation,
            ladderNormal = currentLadderNormal,
            climbing = Climbing,
            crouching = Crouching,
            crouchAmount = Crouching ? 0f : 1f,
            grounded = Grounded,
            submerged = submerged,
        });
        if (preventInput)
        {
            smoothedInputs.mouseHorizontal = 0;
            smoothedInputs.mouseVertical = 0;
        }
        playerCCMotor.setInputs(ref smoothedInputs);
        Grounded = playerCCMotor.motor.GroundingStatus.IsStableOnGround;

        currentlyLookingAtPoint = playerCombatController.RaycastFromCamera();
        playerCombatController.UpdateWeapon();
        lookTargetTransform.position = currentlyLookingAtPoint;
        //playerCameraMotor.Tick(LookInput, Time.deltaTime);
        //playerCCMotor.Move(finalMove * Time.deltaTime);
        playerAnimationController.updateMovemementParams(smoothedInputs);

        playerCameraMotor.Tick();
        playerAnimationController.Tick();
        CheckCameraTranstions();
    }

    public void LateUpdate()
    {
        if (!IsLocalPlayer) return;

        // Update the camera using the new staged system
        //playerCameraMotor.LateTick();

        // Continue updating animations
        playerCameraMotor.LateTick();
        playerAnimationController.LateTick();

        playerCamera.transform.rotation = playerCameraMotor.HandleRotation(playerCamera.transform.rotation, Time.deltaTime, new Vector3(smoothedInputs.mouseHorizontal, smoothedInputs.mouseVertical));
        playerCamera.transform.position = playerCameraMotor.HandlePosition(Time.deltaTime, aiming, playerCamera.transform.rotation, playerCamera.transform.position);

    }

    void OnAim(bool pressed)
    {
        if (!IsLocalPlayer) return;
        if (Climbing || !Grounded || Submerged)
        {
            aiming = false;
            return;
        }
        aiming = pressed;
        if (pressed)
        {
            if (playerCombatController.currentWeapon == null) return;
            if (!playerCombatController.currentWeapon.baseitem.canADS)
            {
                return;
            }
            if (playerCombatController.currentWeapon.baseitem.WeaponType == WeaponType.melee || playerCombatController.currentWeapon.baseitem.WeaponType == WeaponType.throwable) return;
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
            //if(playerCameraMotor.currentState == playerCameraMotor.availableStates[CameraStates.Aiming]) 
        }
    }
    public void CheckCameraTranstions()
    {
        // 1. Enforce Restriction: Cannot aim if Climbing or Submerged
        if ((Climbing || Submerged) && aiming)
        {
            aiming = false;
        }

        // 2. Determine state based on priority
        CameraStates targetState = CameraStates.Default;

        if (Submerged)
        {
            targetState = CameraStates.Submerged;
        }
        else if (Climbing)
        {
            targetState = CameraStates.Climbing;
        }
        else if (aiming)
        {
            // If aiming and crouching, use AimingCrouch, otherwise regular Aiming
            targetState = Crouching ? CameraStates.AimingCrouch : CameraStates.Aiming;
        }
        else if (Crouching)
        {
            targetState = CameraStates.Crouch;
        }

        // 3. Apply Transition
        CameraState targetStateInstance = playerCameraMotor.availableStates[targetState];
        if (playerCameraMotor.currentState != targetStateInstance)
        {
            playerCameraMotor.transition(targetStateInstance);
        }
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

    public void toggleFiremode(bool input)
    {
        playerCombatController.toggleFiremode();
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



