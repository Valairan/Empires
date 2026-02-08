using System.Net;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class UiController : MonoBehaviour
{
    public static UiController Singleton;

    [Header("Current Player")]
    public BuildHandler currentPlayerBuildHandler;
    public InventoryHandler currentPlayerInventoryHandler;
    [Header("Properties Display")]
    [SerializeField] Image Health;
    [SerializeField] Image Armor;
    [SerializeField] Image InteractionProgress;
    [SerializeField] Image InteractionPrompt;
    [SerializeField] GameObject currentlyLookingAtParent;
    [SerializeField] TMP_Text currentlyLookingAtLabel;
    [SerializeField] TMP_Text currentlyLookingAtDescription;
    [Header("In Game Display")]
    [SerializeField] GameObject InGame;
    [SerializeField] GameObject BaseInGame;
    [SerializeField] GameObject ScopedIn;
    [SerializeField] Image ScopedInTexture;
    [SerializeField] Image currentlyEquipped;
    [SerializeField] GameObject ammoInTotal;
    [SerializeField] GameObject ammoInGun;
    [Header("Pause Display")]
    [SerializeField] GameObject PauseMenu;

    [Header("Build Menu Display")]
    [SerializeField] GameObject buildablePlacementValid;
    [SerializeField] GameObject buildMenu;
    [SerializeField] TMP_Text buildableName;
    [SerializeField] TMP_Text buildableDescription;
    [Header("Weapons Menu Display")]
    [SerializeField] GameObject weaponsMenu;
    [SerializeField] TMP_Text weaponName;
    [SerializeField] TMP_Text weaponDescription;


    public void Awake()
    {
        if (Singleton == null) Singleton = this;
    }

    public void onAim(bool pressed)
    {
        BaseInGame.SetActive(!pressed);
    }
    public void setCurerntlyLookingAt(Item item, Vector3 itemPosition)
    {
        if (item != null)
        {
            currentlyLookingAtParent.SetActive(true);
            currentlyLookingAtLabel.text = item.name;
            currentlyLookingAtDescription.text = item.ItemDescription;
            InteractionPrompt.rectTransform.position = Camera.main.WorldToScreenPoint(itemPosition);
        }
        else
        {
            currentlyLookingAtParent.SetActive(false);
            currentlyLookingAtLabel.text = "";
            currentlyLookingAtDescription.text = "";
            InteractionPrompt.rectTransform.position = Camera.main.WorldToScreenPoint(itemPosition);
        }
    }
    public void setHealth(float health)
    {
        Health.fillAmount = health;
    }
    public void setInteractionProgress(float interactionProgress)
    {
        InteractionProgress.fillAmount = interactionProgress;
    }
    public void displayInteractIcon(bool display, Vector3 worldPosition)
    {

        InteractionPrompt.gameObject.SetActive(display);
        InteractionPrompt.rectTransform.position = Camera.main.WorldToScreenPoint(worldPosition);
    }

    public void displayHoveringBuildableInformation(Item item)
    {
        buildableName.text = item.name;
        buildableDescription.text = item.ItemDescription;
    }
    public void toggleInGameUI()
    {
        InGame.SetActive(!buildMenu.activeSelf);
    }
    public void toggleBuildMenu()
    {
        buildMenu.SetActive(!buildMenu.activeSelf);
    }
    public void setCurrentBuildable(Machine machine)
    {
        currentPlayerBuildHandler.setCurrentMachine(machine);
        if (currentPlayerBuildHandler.startPreview())
            toggleBuildMenu();
    }
    public void purchaseWeapon(Weapon weapon)
    {
        //weapon.OnBuy(NetworkManager.Singleton.LocalClientId);
        toggleWeaponSelector();
    }

    public void canBuildSomethingButtonToggle()
    {

    }
    public void weaponChanged(Weapon weapon)
    {
        switch (weapon.WeaponType)
        {
            case WeaponType.melee: ammoInGun.SetActive(false); ammoInTotal.SetActive(false); break;
            case WeaponType.rifle: case WeaponType.sidearm: ammoInGun.SetActive(true); ammoInTotal.SetActive(true); ; break;
        }
        currentlyEquipped.sprite = weapon.ItemIcon;
        if (weapon.WeaponType != WeaponType.melee)
            ScopedInTexture.sprite = ((RangedWeapon)weapon).scopeTexture;
    }
    public void firerateAndMagazineChanged()
    {

    }

    public void toggleWeaponSelector()
    {
        weaponsMenu.SetActive(!weaponsMenu.activeSelf);

    }

    public void buildableLocationValid(bool valid)
    {
        buildablePlacementValid.SetActive(!valid);

    }
    public void togglePauseMenu(bool valid)
    {
        PauseMenu.SetActive(!PauseMenu.activeSelf);

    }
}