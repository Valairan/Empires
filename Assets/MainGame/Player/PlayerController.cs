using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [Header("Components")]
    [SerializeField] CharacterController playerCCMotor;
    [SerializeField] SkinnedMeshRenderer playerClothesParent;

    [Header("Locomotion Settings")]
    [SerializeField] float runSpeed;
    [SerializeField] float walkSpeed;

    [Header("Camera Settings")]
    [SerializeField] Camera playerCamera;
    [SerializeField] Transform playerCameraParent;
    [SerializeField] Transform playerCameraOrbit;

    public void Update()
    {

    }

    public void LateUpdate()
    {
        
    }

}
