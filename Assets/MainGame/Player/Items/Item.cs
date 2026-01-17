using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/New Item")]
public class Item : ScriptableObject
{
    public String ItemName;
    public String ItemDescription;
    public bool stack;
}