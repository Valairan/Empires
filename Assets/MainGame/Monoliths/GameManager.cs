using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
public class GameManager : NetworkBehaviour
{
    [SerializeField] bool generateTerrain;
    [SerializeField] Transform[] playerSpawnPositions;
    [SerializeField] Transform playerCanvas;
    [SerializeField] WorldGenerator worldGenerator;
    public Action sceneLoadComplete;
    public static GameManager Singleton;
    float loaderProgress = 0;

    [Header("Clock")]
    private bool GameStarted = false;
    [SerializeField] private float dayLengthInSeconds = 1200f;


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

    void Update()
    {
        if (!IsServer) return;
        if (!GameStarted) return;
        UpdateNetworkClock();
    }
    private void UpdateNetworkClock()
    {
        if (NetworkGamePropertiesStorage.Singleton == null) return;

        // Calculate normalized time delta based on frame time
        float timeDelta = Time.deltaTime / dayLengthInSeconds;

        // Increment, wrapping seamlessly between 0.0f and 1.0f
        float newTime = (NetworkGamePropertiesStorage.Singleton.CurrentTime.Value + timeDelta) % 1.0f;
        NetworkGamePropertiesStorage.Singleton.CurrentTime.Value = newTime;
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
        GameStarted = true;

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
    public void StartGame_ClientRpc()
    {
        Loader.Singelton.gameObject.SetActive(false);
        GameStarted = true;
        // You can use this space on the client if you need to run specific initialization 
        // that must occur the exact moment the loading screen fades away.
        Debug.Log("Game active! Local mechanics initialized.");
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
