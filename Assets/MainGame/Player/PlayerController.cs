using System;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] CharacterController playerCCMotor;

    [Header("Locomotion Settings")]
    [SerializeField] float runSpeed;
    [SerializeField] float walkSpeed;

    [Header("Camera Settings")]
    [SerializeField] Camera playerCamera;
    [SerializeField] Transform playerCameraParent;
    [SerializeField] Transform playerCameraOrbit;

    public void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            playerCCMotor.Move(transform.forward);
        }
    }

    public void LateUpdate()
    {

    }

}
