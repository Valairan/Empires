using Unity.Netcode;
using UnityEngine;

public class ItemBehaviour : NetworkBehaviour
{
    [Header("Base Item")]
    [SerializeField] public Item baseitem;
}