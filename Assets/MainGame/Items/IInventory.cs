using Unity.Netcode;

public interface IInventory
{
    public void PickupWeapon(Weapon weapon, NetworkBehaviour inworld);
    public void AddItem();
}

