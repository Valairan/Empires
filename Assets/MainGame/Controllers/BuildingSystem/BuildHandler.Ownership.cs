using UnityEngine;

public partial class BuildHandler
{

}

public interface IBuildContext
{
    bool RegisterBuilding(ulong interactor, ulong building);
    bool ValidateOwnership(ulong interactor, ulong building);
    bool CanPlaceStructure(ulong interactor, ulong building);
    bool CanInteractWithStructure(ulong interactor, ulong building);
    bool ChangeOwnership(ulong source, ulong destination, ulong item);
}

public interface IBuildDatabaseContext
{
    GameObject GetPrefab(string prefab);
    int GetDatabaseCount();
}
