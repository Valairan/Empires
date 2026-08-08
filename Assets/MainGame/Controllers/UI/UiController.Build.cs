using TMPro;
using UnityEngine;

public partial class UiController
{
    [Header("Build Menu Display")]
    [SerializeField] GameObject buildablePlacementValid;
    [SerializeField] GameObject buildMenu;
    [SerializeField] TMP_Text buildableName;
    [SerializeField] TMP_Text buildableDescription;
    [SerializeField] GameObject BuildControls;

    public void toggleBuildMenu()
    {
        toggleInGameUI();
        buildMenu.SetActive(!buildMenu.activeSelf);
    }
    public void setCurrentBuildable(Machine machine)
    {
        currentPlayerBuildHandler.setCurrentMachine(machine);
        if (!currentPlayerBuildHandler.startPreview())
        {
            toggleBuildMenu();
        }
        else
        {
            enabledBuildControls();
        }
    }
    public void displayBuildPacementValid(bool input)
    {
        buildablePlacementValid.SetActive(input);
    }

    public void disableBuildControls()
    {
        BuildControls.SetActive(false);
    }
    public void enabledBuildControls()
    {
        BuildControls.SetActive(true);
    }

}