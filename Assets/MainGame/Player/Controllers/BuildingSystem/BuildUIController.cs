using UnityEngine;

public class BuildUIController : MonoBehaviour
{
    public void setCurrentBuildable(Machine machine)
    {
        UiController.Singleton.setCurrentBuildable(machine);
    }

}
