using UnityEngine;
using UnityEngine.UI;

public class BuildHandler : MonoBehaviour
{

    public GameObject buildMenuUI;

    public Machine[] allAvailableMachines;
    public Machine testItem;
    private Machine toBuild;
    private GameObject toBuildGO;
    private Buildable toBuildBuildable;

    bool inPreview = false;

    public void toggleBuildMenu()
    {
        buildMenuUI.SetActive(!buildMenuUI.activeSelf);
    }

    void Update()
    {
        if (inPreview)
        {
            toBuildGO.transform.position = transform.position + transform.forward;
        }
    }
    public void buildButtonPressed()
    {
        //if (toBuild == null && !inPreview)
        //    toggleBuildMenu();
        if (!inPreview)
        {
            toBuildGO = Instantiate(testItem.machinePrefab);
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

        toBuildBuildable = null;
        inPreview = false;
    }
}
