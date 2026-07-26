using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Machine", menuName = "Empires/Machines/New Machine")]
public class Machine : Item
{
    public GameObject machinePrefab;
    public float buildTime;
    public GameObject preview;
    public Item[] canManufacture;
    public LayerMask buildableLayers;
    public LayerMask blockingLayers;

    public Vector3Int footprint = Vector3Int.one;
    public bool requiresGroundSupport = true;
    public bool canBuildOnSlopes = false;
    public float maxSlopeAngle = 30f;
    public bool isPrivate = true;
    public bool isBuilding = true;
    public bool allowExtraRotation = false;
    public bool canBuildOnWater;
    public bool requiresGround;
}