using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [Header("Components")]
    [SerializeField] CharacterController playerCCMotor;
    [SerializeField] Animator playerAnimator;
    [SerializeField] SkinnedMeshRenderer playerClothesParent;
    [SerializeField] InputHandler playerInputHandler;

    [Header("Locomotion Settings")]
    bool Grounded;
    [SerializeField] float runSpeed;
    [SerializeField] float walkSpeed;
    [SerializeField] Transform GroundCheck;
    [SerializeField] LayerMask WhatIsGround;
    Vector3 MoveDirection;

    [Header("Camera Settings")]
    [SerializeField] Camera playerCamera;
    [SerializeField] Transform playerCameraParent;
    [SerializeField] Transform playerCameraOrbit;

    ulong clientID;
    public override void OnNetworkSpawn()
    {
        //if (!IsLocalPlayer) return;
        //playerCamera.gameObject.SetActive(true);
        //clientID = NetworkManager.Singleton.LocalClientId;
    }
    public void Update()
    {
        if (!IsLocalPlayer) return;
        MoveDirection = new Vector3(playerInputHandler.MoveInput.x * (playerInputHandler.Sneak ? walkSpeed : runSpeed), 0, playerInputHandler.MoveInput.y * (playerInputHandler.Sneak ? walkSpeed : runSpeed));
        playerCCMotor.Move(MoveDirection * Time.deltaTime);
        Collider[] groundColliders = Physics.OverlapSphere(GroundCheck.position, .2f, WhatIsGround);
        if (groundColliders.Length > 0)
            Grounded = true;
        else
            Grounded = false;


        updateAnimationParams(playerInputHandler.MoveInput, false, false, false, false);
    }

    public void LateUpdate()
    {
        if (!IsLocalPlayer) return;

    }

    public void updateAnimationParams(Vector2 movement, bool grounded, bool sideArm, bool rifle, bool melee)
    {
        playerAnimator.SetFloat("Horizontal", movement.x);
        playerAnimator.SetFloat("Vertical", movement.y);
    }

}
