
using Unity.Netcode;
using UnityEngine;

public class ResourcesManager : NetworkBehaviour
{
    public static ResourcesManager Singleton;
    public WorldGenerator worldGenerator;

    public BaseResource[,] resources;

    void Awake()
    {
        if (Singleton == null) Singleton = this;
        int grassDistance = PlayerPrefs.GetInt("GrassDistance", 4);
        worldGenerator.numberOfChunksToRender = (int)grassDistance;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DamageOre_ServerRpc(float damage, int x, int y, ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;
        if (damage <= 0) return;
        BaseResourceBehaviour currentTree = worldGenerator.placedTrees[x, y];
        NetworkObject sender = NetworkManager.Singleton.ConnectedClients[rpcParams.Receive.SenderClientId].PlayerObject;
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
                for (int j = 0; j < 5; j++)
                {
                    if (res.dropsHowMany[i] <= 0) break;
                    position.x += UnityEngine.Random.Range(-.1f, .1f);
                    position.z += UnityEngine.Random.Range(-.1f, .1f);
                    position.y += 1;
                    GameObject temp = Instantiate(res.drops[i], position, Quaternion.identity);
                    temp.GetComponent<NetworkObject>().Spawn();
                    res.dropsHowMany[i] -= 1;
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
        Debug.Log("on server");
        if (damage <= 0) return;
        BaseResourceBehaviour currentTree = worldGenerator.placedTrees[x, y];
        NetworkObject sender = NetworkManager.Singleton.ConnectedClients[rpcParams.Receive.SenderClientId].PlayerObject;
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

    [ClientRpc]
    public void GenerateTerrain_ClientRpc(int seed)
    {
        TerrainSettings settings = new TerrainSettings();
        settings.mapWidth = 1000;
        settings.mapHeight = 1000;
        settings.seed = seed;
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
}