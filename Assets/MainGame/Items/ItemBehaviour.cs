using Unity.Netcode;
using UnityEngine;

public class ItemBehaviour<T> : NetworkBehaviour where T : Item
{
    [Header("Base Item")]
    [SerializeField] public T baseitem;
    public T Item => baseitem;

    public virtual void InitializeItem()
    {
        
    }

}
