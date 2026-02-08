using Unity.Netcode;

public interface IInventory
{
    public void EquipWeapon(Weapon weapon, NetworkBehaviour inworld);
    public void AddItem();
}

