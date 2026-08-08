using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class UiController
{
    [Header("Weapons Menu Display")]
    [SerializeField] GameObject weaponsMenu;
    [SerializeField] TMP_Text weaponName;
    [SerializeField] TMP_Text weaponDescriptionCol1;
    [SerializeField] TMP_Text weaponDescriptionCol2;

    string RangedWeaponDetailsTemplate = "FireRate: {0} \n ";
    string RangedWeaponDamageTemplate = "HEAD: {0,15}\nBODY: {1,15}\nLEGS: {2,15}";

    public void purchaseWeapon(Weapon weapon)
    {
        //weapon.OnBuy(NetworkManager.Singleton.LocalClientId);
        toggleInGameUI();
        toggleWeaponSelector();
    }

    public void showWeaponDetails(Weapon weapon)
    {
        if (weapon is RangedWeapon rangedWeapon)
            weaponDescriptionCol2.text = string.Format(RangedWeaponDamageTemplate, rangedWeapon.headDamage, rangedWeapon.bodyDamage, rangedWeapon.legDamage);
    }
    public void clearWeaponDetails()
    {
        weaponDescriptionCol1.text = "";
        weaponDescriptionCol2.text = "";
    }

    public void toggleWeaponSelector()
    {
        weaponsMenu.SetActive(!weaponsMenu.activeSelf);

    }
}