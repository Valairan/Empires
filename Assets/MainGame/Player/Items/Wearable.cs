using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/New Wearable")]
public class Wearable : Item
{
    public int id;
    public GameObject WorldPrefab;
    public SkinnedMeshRenderer wearableMesh;
    public GameObject WearablePrefab;
    public Stats stats;

}

[Serializable]
public struct Stats
{
    public int shieldTotal;
    public int healthOnGround;
}