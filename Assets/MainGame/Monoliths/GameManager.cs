using System;
using Unity.Netcode;
using UnityEngine;
public class GameManager : NetworkBehaviour
{
    [SerializeField] bool generateTerrain;
    [SerializeField] Transform[] playerSpawnPositions;
    [SerializeField] Transform playerCanvas;
    [SerializeField] WorldGenerator worldGenerator;

    public Action sceneLoadComplete;
    public static GameManager Singleton;
    float loaderProgress = 0;

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
        if (generateTerrain) GenerateTerrain_ClientRpc(NetworkGamePropertiesStorage.Singleton.WorldGenerationSeed.Value);
        else
        {
            
        }
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

    [ClientRpc]
    public void GenerateTerrain_ClientRpc(int seed)
    {
        TerrainSettings settings = new TerrainSettings
        {
            mapWidth = 1000,
            mapHeight = 1000,
            seed = seed,
            scale = 32,
            octaves = 4,
            persistance = 4,
            lacunarity = 0,
            multiplier = 5,
            offset = Vector2.zero,
            falloffHeight = 20,
            falloffDistance = 5
        };
        worldGenerator.GenerateTerrain(settings);

    }
}
// Item retrieveFromAssetDatabase(int id)
// {

// }
