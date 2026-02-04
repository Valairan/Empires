using System;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
public class GameManager : NetworkBehaviour
{
    [SerializeField] WorldGenerator worldGenerator;
    [SerializeField] Transform[] playerSpawnPositions;
    [SerializeField] Transform playerCanvas;

    public Action sceneLoadComplete;
    public static GameManager Singleton;
    public BaseResource[,] resources;
    float loaderProgress = 0;
    public GameObject testPrefab;

    void Awake()
    {
        if (Singleton == null) Singleton = this;
    }

    public override void OnNetworkSpawn()
    {
        sceneLoadComplete += init;
        int grassDistance = PlayerPrefs.GetInt("GrassDistance", 4);
        worldGenerator.numberOfChunksToRender = (int)grassDistance;
    }

    public override void OnNetworkDespawn()
    {
        sceneLoadComplete -= init;
    }


    public void init()
    {
        if (!IsServer) return;

        NetworkGamePropertiesStorage.Singleton.WorldGenerationSeed.Value = UnityEngine.Random.Range(0, 2000);
        GenerateTerrain_ClientRpc();

        int count = 0;
        foreach (ulong client in NetworkManager.ConnectedClientsIds)
        {
            GameObject player = Instantiate(NetworkGamePropertiesStorage.Singleton.playerPrefab);

            PlayerController controller = player.GetComponent<PlayerController>();

            NetworkGamePropertiesStorage.Singleton.spawnedPlayers.Add(client, player);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(client, true);

            count++;
            loaderProgress = count / NetworkManager.Singleton.ConnectedClients.Count;
            SetLoader_ClientRpc(loaderProgress);
        }
        for (int i = 0; i < 10; i++)
        {
            Instantiate(testPrefab, playerSpawnPositions[0].position, Quaternion.identity).GetComponent<NetworkObject>().Spawn();
            Instantiate(testPrefab, playerSpawnPositions[1].position, Quaternion.identity).GetComponent<NetworkObject>().Spawn();
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
    public void GenerateTerrain_ClientRpc()
    {
        TerrainSettings settings = new TerrainSettings();
        settings.mapWidth = 1000;
        settings.mapHeight = 1000;
        settings.seed = NetworkGamePropertiesStorage.Singleton.WorldGenerationSeed.Value;
        settings.scale = 32;
        settings.octaves = 4;
        settings.persistance = 4;
        settings.lacunarity = 0;
        settings.multiplier = 5;
        settings.offset = Vector2.zero;
        settings.falloffHeight = 20;
        settings.falloffDistance = 5;
        worldGenerator.GenerateTerrain(settings);

    }



    [ClientRpc]
    public void DisableLoader_ClientRpc()
    {
        Loader.Singelton.gameObject.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DamageOre_ServerRpc(float damage, int x, int y, ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;
        if (damage <= 0) return;
        BaseResourceBehaviour currentTree = worldGenerator.placedTrees[x, y];
        NetworkGamePropertiesStorage.Singleton.spawnedPlayers.TryGetValue(rpcParams.Receive.SenderClientId, out GameObject sender);
        float health = currentTree.health.amount;
        if (Vector3.Distance(sender.transform.position, currentTree.transform.position) > 2)
            return;
        health -= damage;
        Vector3 position = currentTree.transform.position;
        if (((BaseResource)currentTree.baseitem).drops.Length > 0)
        {
            BaseResource res = (BaseResource)currentTree.baseitem;
            for (int i = 0; i < res.drops.Length; i++)
            {
                for (int j = 0; j < res.dropsHowMany[i]; j++)
                {
                    position.x += UnityEngine.Random.Range(-.1f, .1f);
                    position.z += UnityEngine.Random.Range(-.1f, .1f);
                    position.y += 1;
                    GameObject temp = Instantiate(res.drops[i], position, Quaternion.identity);
                    temp.GetComponent<NetworkObject>().Spawn();
                }
            }
        }
        if (currentTree.health.amount <= 0f)
        {
            killResource_ClientRpc(x, y);
            return;
        }
        currentTree.health.amount = health;
        updateResource_ClientRpc(x, y, health);

    }
    [ServerRpc(RequireOwnership = false)]
    public void DamageTree_ServerRpc(float damage, int x, int y, ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;
        if (damage <= 0) return;
        BaseResourceBehaviour currentTree = worldGenerator.placedTrees[x, y];
        NetworkGamePropertiesStorage.Singleton.spawnedPlayers.TryGetValue(rpcParams.Receive.SenderClientId, out GameObject sender);
        float health = currentTree.health.amount;
        if (Vector3.Distance(sender.transform.position, currentTree.transform.position) > 2)
            return;
        health -= damage;
        if (health <= 0f)
        {
            Vector3 position = currentTree.transform.position;
            if (((BaseResource)currentTree.baseitem).drops.Length > 0)
            {
                BaseResource res = (BaseResource)currentTree.baseitem;
                for (int i = 0; i < res.drops.Length; i++)
                {
                    for (int j = 0; j < res.dropsHowMany[i]; j++)
                    {
                        position.y += 1;
                        GameObject temp = Instantiate(res.drops[i], position, Quaternion.identity);
                        temp.GetComponent<NetworkObject>().Spawn();
                    }
                }
            }

            killResource_ClientRpc(x, y);
            return;
        }
        updateResource_ClientRpc(x, y, health);

    }

    [ClientRpc]
    public void updateResource_ClientRpc(int x, int y, float newHealth)
    {
        worldGenerator.placedTrees[x, y].health.amount = newHealth;
    }
    [ClientRpc]
    public void killResource_ClientRpc(int x, int y)
    {
        Destroy(worldGenerator.placedTrees[x, y].gameObject);
        worldGenerator.placedTrees[x, y] = null;
    }
}
// Item retrieveFromAssetDatabase(int id)
// {

// }
