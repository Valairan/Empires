using TMPro;
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
    [SerializeField] Image[] InventoryIcons;
    [SerializeField] CanvasGroup[] InventoryIconsOpacity;
    [SerializeField] TMP_Text[] InventoryNames;
    [SerializeField] GameObject ammoInTotal;
    [SerializeField] GameObject ammoInGun;
    [Header("Inventory Display")]
    [SerializeField] GameObject Inventory;
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
        if (ScopedInTexture.sprite)
            ScopedIn.SetActive(pressed);
    }

    public void init()
    {
        InGame.SetActive(true);
    }
    public void setCurerntlyLookingAt(Item item, Vector3 itemPosition)
    {
        if (item != null)
        {
            currentlyLookingAtParent.SetActive(true);
            currentlyLookingAtLabel.text = item.ItemName;
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
        Health.fillAmount = health / 100;
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
    public void toggleInventory()
    {
        toggleInGameUI();
        Inventory.SetActive(!Inventory.activeSelf);
    }
    public void toggleInGameUI()
    {
        InGame.SetActive(!InGame.activeSelf);
    }
    public void toggleBuildMenu()
    {
        toggleInGameUI();
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
        toggleInGameUI();
        toggleWeaponSelector();
    }

    public void canBuildSomethingButtonToggle()
    {

    }

    bool hasRifle = false;
    bool hasSidearm = false;
    bool hasMelee = false;
    public void updateInventoryDisplay(int currentlyEquipped)
    {

        foreach (var slot in currentPlayerInventoryHandler.weaponStorage)
        {
            var weapon = slot.weapon;

            switch (weapon.WeaponType)
            {
                case WeaponType.rifle:
                    InventoryIconsOpacity[0].alpha = (currentlyEquipped == 0) ? 1f : .2f;
                    InventoryIcons[0].sprite = weapon.ItemIcon;
                    InventoryNames[0].text = weapon.ItemName;
                    InventoryIcons[0].transform.parent.gameObject.SetActive(true);
                    hasRifle = true;
                    break;

                case WeaponType.sidearm:
                    InventoryIconsOpacity[1].alpha = (currentlyEquipped == 1) ? 1f : .2f;
                    InventoryIcons[1].sprite = weapon.ItemIcon;
                    InventoryNames[1].text = weapon.ItemName;
                    InventoryIcons[1].transform.parent.gameObject.SetActive(true);
                    hasSidearm = true;
                    break;

                case WeaponType.melee:
                    InventoryIconsOpacity[2].alpha = (currentlyEquipped == 2) ? 1f : .2f;
                    InventoryIcons[2].sprite = weapon.ItemIcon;
                    InventoryNames[2].text = weapon.ItemName;
                    InventoryIcons[2].transform.parent.gameObject.SetActive(true);
                    hasMelee = true;
                    break;
            }
        }


        if (!hasRifle)
            InventoryIcons[0].transform.parent.gameObject.SetActive(false);

        if (!hasSidearm)
            InventoryIcons[1].transform.parent.gameObject.SetActive(false);

        if (!hasMelee)
            InventoryIcons[2].transform.parent.gameObject.SetActive(false);
    }


    public void weaponChanged(Weapon weapon)
    {
        if (weapon is RangedWeapon rangedWeapon)
        {
            ScopedInTexture.sprite = rangedWeapon.scopeTexture;
        }
        else
        {
            ScopedInTexture = null;
        }
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

    internal void updateInventoryDisplay(int previousValue, int newValue)
    {
        updateInventoryDisplay(newValue);
    }
}