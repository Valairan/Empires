using UnityEngine;

public partial class BuildHandler 
{
    public void setCurrentBuildable(Machine machine)
    {
        UiController.Singleton.setCurrentBuildable(machine);
    }

}
