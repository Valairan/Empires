using Unity.Netcode;
using UnityEngine;

public class TestNetworkPlayerSpawner : NetworkBehaviour
{
    public void Start()
    {
        if (!IsServer) return;

        foreach (PlayerData client in NetworkGamePropertiesStorage.Singleton.spawnedPlayersNames)
        {
            GameObject player = Instantiate(NetworkGamePropertiesStorage.Singleton.playerPrefab);
            NetworkGamePropertiesStorage.Singleton.spawnedPlayers.Add(client.clientId, player);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(client.clientId, true);
        }
    }
}
