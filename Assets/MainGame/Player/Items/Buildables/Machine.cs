using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Machine", menuName = "Items/Machines/New Machine")]
public class Machine : Item
{
    public GameObject machinePrefab;
    public Buildable buildable;
    public Item[] canManufacture;

}