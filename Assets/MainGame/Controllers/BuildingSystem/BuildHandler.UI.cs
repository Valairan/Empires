using System;

public partial class BuildHandler
{
    public Action<bool> onBuildvalidityChange;
    public void setCurrentBuildable(Machine machine)
    {
        setCurrentMachine(machine);
        UiController.Singleton.setCurrentBuildable(machine);
    }

}