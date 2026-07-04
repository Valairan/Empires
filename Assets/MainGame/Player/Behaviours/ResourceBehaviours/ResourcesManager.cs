
using Unity.Netcode;
using UnityEngine;

public class ResourcesManager : NetworkBehaviour
{
    public static ResourcesManager Singleton;

    public BaseResourceBehaviour[,] placedTrees;

    void Awake()
    {
        if (Singleton == null) Singleton = this;
        int grassDistance = PlayerPrefs.GetInt("GrassDistance", 4);
        //worldGenerator.numberOfChunksToRender = (int)grassDistance;
    }


    


}