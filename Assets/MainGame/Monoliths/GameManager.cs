using System;
using Unity.Netcode;
using UnityEngine;
public class GameManager : NetworkBehaviour
{
    [SerializeField] Transform[] playerSpawnPositions;
    [SerializeField] Transform playerCanvas;

    public Action sceneLoadComplete;
    public static GameManager Singleton;
    float loaderProgress = 0;
    public GameObject testPrefab;
    public GameObject testPrefab2;

    void Awake()
    {
        if (Singleton == null) Singleton = this;
    }

    public override void OnNetworkSpawn()
    {
        sceneLoadComplete += init;

    }

    public override void OnNetworkDespawn()
    {
        sceneLoadComplete -= init;
    }


    public void init()
    {
        if (!IsServer) return;

        NetworkGamePropertiesStorage.Singleton.WorldGenerationSeed.Value = UnityEngine.Random.Range(0, 2000);
        ResourcesManager.Singleton.GenerateTerrain_ClientRpc(NetworkGamePropertiesStorage.Singleton.WorldGenerationSeed.Value);

        int count = 0;
        foreach (ulong client in NetworkManager.ConnectedClientsIds)
        {
            GameObject player = Instantiate(NetworkGamePropertiesStorage.Singleton.playerPrefab);

            PlayerController controller = player.GetComponent<PlayerController>();
            controller.playerCCMotor.motor.SetPosition(GetSpawnPointForClient(client));
            NetworkGamePropertiesStorage.Singleton.spawnedPlayers.Add(client, player);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(client, true);
            count++;
            loaderProgress = count / NetworkManager.Singleton.ConnectedClients.Count;
            SetLoader_ClientRpc(loaderProgress);
        }
        for (int i = 0; i < 5; i++)
        {
            Instantiate(testPrefab, playerSpawnPositions[0].position, Quaternion.identity).GetComponent<NetworkObject>().Spawn();
            Instantiate(testPrefab2, playerSpawnPositions[0].position, Quaternion.identity).GetComponent<NetworkObject>().Spawn();
        }
        for (int i = 0; i < 5; i++)
        {
            Instantiate(testPrefab, playerSpawnPositions[1].position, Quaternion.identity).GetComponent<NetworkObject>().Spawn();
            Instantiate(testPrefab2, playerSpawnPositions[1].position, Quaternion.identity).GetComponent<NetworkObject>().Spawn();
        }
        DisableLoader_ClientRpc();

    }
    public Vector3 GetSpawnPointForClient(ulong clientID)
    {
        return playerSpawnPositions[(int)clientID % 8].position;
    }

    [ClientRpc]
    public void SetLoader_ClientRpc(float value)
    {
        Loader.Singelton.setProgress(value);
    }




    [ClientRpc]
    public void DisableLoader_ClientRpc()
    {
        Loader.Singelton.gameObject.SetActive(false);
    }


}
// Item retrieveFromAssetDatabase(int id)
// {

// }
