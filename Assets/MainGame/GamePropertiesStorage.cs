using UnityEngine;

public class GamePropertiesStorage : MonoBehaviour
{
    public static GamePropertiesStorage Singleton;

    public Item[] EveryItemAndItsCost;
    void Awake()
    {
        if (Singleton == null) Singleton = this;
    }



}
