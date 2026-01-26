using System;
using System.Reflection;
using TMPro;
using TreeEditor;
using Unity.VisualScripting;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.UI;
public class UiController : MonoBehaviour
{
    public static UiController Singleton;

    [Header("Current Player")]
    public BuildHandler currentPlayerBuildHandler;
    [Header("Properties Display")]
    [SerializeField] Image Health;
    [SerializeField] Image Armor;
    [SerializeField] Image InteractionProgress;
    [SerializeField] Image InteractionPrompt;
    [SerializeField] GameObject currentlyLookingAtParent;
    [SerializeField] TMP_Text currentlyLookingAtLabel;
    [Header("Build Menu Display")]
    [SerializeField] GameObject buildMenu;
    [SerializeField] TMP_Text buildableName;
    [SerializeField] TMP_Text buildableDescription;


    public void Awake()
    {
        if (Singleton == null) Singleton = this;
    }

    public void setCurerntlyLookingAt(Item item, Vector3 itemPosition)
    {
        if (item != null)
        {
            currentlyLookingAtParent.SetActive(true);
            currentlyLookingAtLabel.text = item.name + "\n" + item.ItemDescription;
            InteractionPrompt.rectTransform.position = Camera.main.WorldToScreenPoint(itemPosition);
        }
        else
        {
            currentlyLookingAtParent.SetActive(false);
            currentlyLookingAtLabel.text = "";
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
    public void toggleBuildMenu()
    {
        buildMenu.SetActive(!buildMenu.activeSelf);
    }
    public void setCurrentBuildable(Machine machine)
    {
        currentPlayerBuildHandler.setCurrentBuildable(machine);
        toggleBuildMenu();
        currentPlayerBuildHandler.startPreview();
    }

    public void canBuildSomethingButtonToggle()
    {

    }
}