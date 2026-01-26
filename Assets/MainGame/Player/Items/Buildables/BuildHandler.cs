using UnityEngine;
using UnityEngine.UI;

public class BuildHandler : MonoBehaviour
{


    public Machine[] allAvailableMachines;
    private Machine toBuild;
    private GameObject toBuildGO;
    private Buildable toBuildBuildable;

    bool inPreview = false;

    void Update()
    {
        if (inPreview)
        {
            toBuildGO.transform.position = transform.position + (transform.forward * 2);
        }
    }
    public void toggleBuildMenu()
    {
        UiController.Singleton.toggleBuildMenu();
    }

    public void startPreview()
    {

    }
    public void buildButtonPressed()
    {
        if (toBuild == null && !inPreview)
        {
            toggleBuildMenu();
            return;
        }

        if (!inPreview)
        {
            toBuildGO = Instantiate(toBuild.machinePrefab);
            toBuildBuildable = toBuildGO.GetComponent<Buildable>();
            inPreview = true;
            return;
        }
        if (inPreview) tryPlaceBuilding();

    }

    public void tryPlaceBuilding()
    {
        if (toBuildBuildable.IsValidPlacement)
        {
            if (toBuildBuildable.TryPlace())
            {
                toBuildBuildable = null;
            }
        }

        toBuild = null;
        toBuildGO = null;
        toBuildBuildable = null;
        inPreview = false;
    }

    public void setCurrentBuildable(Machine machine)
    {
        toBuild = machine;
    }
}
