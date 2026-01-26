using Unity.Netcode;
using UnityEngine;
public class GameManager : NetworkBehaviour
{
    [SerializeField] WorldGenerator worldGenerator;
    [SerializeField] Transform[] playerSpawnPositions;
    [SerializeField] Transform playerCanvas;

    public BaseResource[,] resources;
    float loaderProgress = 0;
    public void Start()
    {
        //Loader.Singelton.gameObject.SetActive(true);
        //Loader.Singelton.setProgress(loaderProgress);
        if (!IsServer) return;
        NetworkGamePropertiesStorage.Singleton.WorldGenerationSeed.Value = Random.Range(0, 2000);
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
            DisableLoader_ClientRpc();
        }
        StartGame_ClientRpc();
    }
    [ClientRpc]
    public void DamageResource_ClientRpc(int x, int y)
    {
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
