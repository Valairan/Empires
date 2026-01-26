using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tree", menuName = "Items/New Tree")]
public class TreeItem : BaseResource
{

    public DamageLookup[] damageLookup;
    public ResourceDrop[] drops;
    public int[] dropsHowMany;
}

[Serializable]
public struct DamageLookup
{
    public Weapon wepaon;
    public float damage;
}