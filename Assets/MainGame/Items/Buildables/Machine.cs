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
}