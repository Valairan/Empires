using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
public class GameManager : NetworkBehaviour
{
    [SerializeField] WorldGenerator worldGenerator;
    [SerializeField] Transform[] playerSpawnPositions;
    [SerializeField] Transform playerCanvas;

    public static GameManager Singleton;
    public BaseResource[,] resources;
    float loaderProgress = 0;
    public GameObject testPrefab;

    void Awake()
    {
        if (Singleton == null) Singleton = this;
    }

    public void Start()
    {

        if (!IsServer) return;
        NetworkGamePropertiesStorage.Singleton.WorldGenerationSeed.Value = UnityEngine.Random.Range(0, 2000);
        int count = 0;
        foreach (PlayerData client in NetworkGamePropertiesStorage.Singleton.connectedPlayerData)
        {
            GameObject player = Instantiate(NetworkGamePropertiesStorage.Singleton.playerPrefab, playerSpawnPositions[count].position, Quaternion.identity);
            NetworkGamePropertiesStorage.Singleton.spawnedPlayers.Add(client.clientId, player);
            PlayerController temp = player.transform.GetComponent<PlayerController>();
            //temp.setClientId(client.clientId);
            //temp.wearItem(retrieveFromAssetDatabase(playerData.head, body, shoe))
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(client.clientId, true);
            count++;
            loaderProgress = count / NetworkManager.Singleton.ConnectedClients.Count;
            SetLoader_ClientRpc();
        }
        Instantiate(testPrefab, new Vector3(500, 30, 500), Quaternion.identity).GetComponent<NetworkObject>().Spawn();
        StartGame_ClientRpc();
    }

    [ClientRpc]
    public void SetLoader_ClientRpc()
    {
        Loader.Singelton.setProgress(loaderProgress);
    }
    [ClientRpc]
    public void StartGame_ClientRpc()
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
        DisableLoader_ClientRpc();

    }

    [ClientRpc]
    public void DisableLoader_ClientRpc()
    {
        Loader.Singelton.gameObject.SetActive(false);
    }

    [ServerRpc]
    public void DamageTree_ServerRpc(float damage, int x, int y, ServerRpcParams rpcParams = default)
    {
        BaseResourceBehaviour currentTree = worldGenerator.placedTrees[x, y];
        NetworkGamePropertiesStorage.Singleton.spawnedPlayers.TryGetValue(rpcParams.Receive.SenderClientId, out GameObject sender);
        float health = currentTree.health.amount;
        if (Vector3.Distance(sender.transform.position, currentTree.transform.position) < 2)
        {
            Debug.Log("The potato has reached the server");
            Debug.Log("x: " + x + ", y: " + y);
            health -= damage;
        }
        if (currentTree.health.amount <= 0f)
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

            killtree_ClientRpc(x, y);
            return;
        }
        updateTreeProperties_ClientRpc(x, y, health);

    }



    [ClientRpc]
    public void updateTreeProperties_ClientRpc(int x, int y, float newHealth)
    {
        worldGenerator.placedTrees[x, y].health.amount = newHealth;
    }
    [ClientRpc]
    public void killtree_ClientRpc(int x, int y)
    {
        Destroy(worldGenerator.placedTrees[x, y].gameObject);
    }
}
// Item retrieveFromAssetDatabase(int id)
// {

// }
