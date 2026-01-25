using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : ItemBehaviour, IRaycastResponder, IDamageable
{
    [Header("Components")]
    [SerializeField] CharacterController playerCCMotor;
    [SerializeField] Animator playerAnimator;
    [SerializeField] SkinnedMeshRenderer playerClothesParent;
    [SerializeField] InputHandler playerInputHandler;
    [SerializeField] BuildHandler playerBuildHandler;
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
    Machine currentlyBuilding;
    IInteractable currentInteractable;
    [HideInInspector] public GameObject currentGO;
    [HideInInspector] public GameObject primaryGO;
    [HideInInspector] public GameObject sidearamGO;
    public GameObject meleeGO;
    [SerializeField] public Transform meleeStorage;
    [SerializeField] public Transform primaryStorage;
    [SerializeField] public Transform sideArmStorage;
    [SerializeField] public Transform equipped;

    public Action<float> onHealthChanged;
    public Action<float> onArmorChanged;
    public Action<float> onInteractionProgressChanged;
    public Action<bool, Vector3> onInteractableInView;
    public Action<Item, Vector3> onLookingAtChanged;
    public Item currentlyLookingAt;
    public ulong clientID;

    public Vector2 MoveInput;
    public Vector2 LookInput;
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
        onHealthChanged += UiController.Singleton != null ? UiController.Singleton.setHealth : null;
        onLookingAtChanged += UiController.Singleton != null ? UiController.Singleton.setCurerntlyLookingAt : null;
        onInteractionProgressChanged += UiController.Singleton != null ? UiController.Singleton.setInteractionProgress : null;
        onInteractableInView += UiController.Singleton != null ? UiController.Singleton.displayInteractIcon : null;

        playerInputHandler.Move += ctx => MoveInput = ctx;
        playerInputHandler.Look += ctx => LookInput = ctx;
        playerInputHandler.Attack += Attack;
        playerInputHandler.Sneak += ctx => sneaking = ctx;
        playerInputHandler.Previous += equipPreviousItem;
        playerInputHandler.Next += equipNextItem;
        playerInputHandler.Jump += jump;

        playerInputHandler.Interact += OnInteract;

        playerInputHandler.Build += buildButtonPressed;


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

        Grounded = Physics.OverlapSphere(GroundCheck.transform.position, 0.1f).Length > 0;

        if (Grounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        // -------- Horizontal movement --------
        if (Grounded)
        {
            Vector3 input = new Vector3(MoveInput.x, 0f, MoveInput.y);

            if (input.sqrMagnitude > 0.01f)
            {
                // Camera-relative strafing
                Vector3 camForward = playerCamera.transform.forward;
                Vector3 camRight = playerCamera.transform.right;

                camForward.y = 0f;
                camRight.y = 0f;

                camForward.Normalize();
                camRight.Normalize();

                Vector3 moveDir = camForward * input.z + camRight * input.x;

                // Rotate toward movement direction
                float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
                float angle = Mathf.SmoothDampAngle(
                    transform.eulerAngles.y,
                    targetAngle,
                    ref turnSmoothVelocity,
                    turnSmoothTime
                );

                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                horizontalVelocity =
                    moveDir.normalized *
                    (sneaking ? walkSpeed : runSpeed);
            }
            else
            {
                horizontalVelocity = Vector3.zero;
            }
        }
        else
        {
            // -------- Air drag (no air control) --------
            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                Vector3.zero,
                3f * Time.deltaTime
            );
        }

        // -------- Jump --------
        if (jumpInput && Grounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // -------- Gravity --------
        velocity.y += gravity * Time.deltaTime;

        // -------- Final Move --------
        Vector3 finalMove =
            horizontalVelocity +
            Vector3.up * velocity.y;

        playerCCMotor.Move(finalMove * Time.deltaTime);
        checkForRaycasts();
        HandleTimedInteraction();
        updateAnimationParams(MoveInput, Grounded, false, false, false, Submerged);

    }

    float interactTimer = 0f;
    void HandleTimedInteraction()
    {
        if (!interacting || currentInteractable == null)
        {
            interactTimer = 0f;
            return;
        }

        interactTimer += Time.deltaTime;

        float duration = currentInteractable.InteractionDuration;
        float progress = Mathf.Clamp01(interactTimer / duration);

        onInteractionProgressChanged?.Invoke(progress);

        if (interactTimer >= duration)
        {
            currentInteractable.Interact(gameObject);
            interactTimer = 0f;
            interacting = false; // require release to interact again
        }
    }
    public void LateUpdate()
    {
        if (!IsLocalPlayer) return;
        playerCamera.transform.position = playerCameraParent.position;
        playerCamera.transform.rotation = playerCameraParent.rotation;
        yaw += LookInput.x * sensitivity * Time.deltaTime;
        pitch -= LookInput.y * sensitivity * Time.deltaTime; // inverted Y

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        playerCameraOrbit.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    public void updateAnimationParams(Vector2 movement, bool grounded, bool sideArm, bool rifle, bool melee, bool inwater)
    {
        if (inwater && (movement.magnitude != 0))
        {
            playerAnimator.SetBool("Submerged", inwater);
            playerAnimator.SetFloat("Horizontal", 1);
            return;
        }
        playerAnimator.SetFloat("Horizontal", movement.x != 0 ? 1 : 0);
        playerAnimator.SetFloat("Vertical", movement.y != 0 ? 1 : 0);
        playerAnimator.SetBool("Grounded", grounded);

    }
    void OnInteract(bool pressed)
    {
        interacting = pressed;

        if (!pressed)
        {
            interactTimer = 0f;
            onInteractionProgressChanged?.Invoke(0f);
        }
    }
    public void checkForRaycasts()
    {
        if (Physics.SphereCast(playerCamera.transform.position, 0.2f, playerCamera.transform.forward, out RaycastHit hit, 25f))
        {
            if (hit.transform.TryGetComponent(out IRaycastResponder responder))
            {
                Item item = responder.respondToRaycast();
                if (item != currentlyLookingAt)
                {
                    currentlyLookingAt = item;
                    onLookingAtChanged?.Invoke(item, hit.point);
                }
                if (hit.transform.TryGetComponent(out IInteractable interactable))
                {
                    currentInteractable = interactable;
                    onInteractableInView.Invoke(true, hit.point);
                }
                else
                {
                    currentInteractable = null;
                    onInteractableInView.Invoke(item, Vector3.zero);
                }
            }
            else
            {
                currentlyLookingAt = null;
                onLookingAtChanged?.Invoke(null, Vector3.zero);

            }

        }

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
    bool jumpInput;
    public void jump(bool started)
    {
        jumpInput = started;
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
