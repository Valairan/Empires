using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
public class NetworkPlayerManager : NetworkBehaviour
{
    public static NetworkPlayerManager Singleton;

    public Dictionary<ulong, PlayerController> connectedPlayerControllers = new Dictionary<ulong, PlayerController>();

    void Awake()
    {
        if (Singleton == null) Singleton = this;

        //worldGenerator.numberOfChunksToRender = (int)grassDistance;
    }

    public void startGame()
    {
        foreach (KeyValuePair<ulong, PlayerController> controller in connectedPlayerControllers)
        {
            controller.Value.health.currentAmount.Value = 50f;
            controller.Value.armor.currentAmount.Value = 0f;
        }
    }




    [ClientRpc]
    public void DamagePlayer_ClientRpc(ulong sender)
    {

    }

}
