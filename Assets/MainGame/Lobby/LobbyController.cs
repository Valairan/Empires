using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;


public class LobbyController : NetworkBehaviour
{
    [SerializeField] TMP_Text[] playerNames;
    [SerializeField] GameObject[] playerIcons;
    [SerializeField] GameObject StartButton;
    [SerializeField] GameObject ReadyButton;

    public void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        NetworkGamePropertiesStorage.Singleton.connectedPlayerData.OnListChanged += assignNamesToScreen;

        if (NetworkManager.Singleton.IsServer)
        {
            initForServer();
        }
    }

    public void OnDisable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        NetworkGamePropertiesStorage.Singleton.connectedPlayerData.OnListChanged -= assignNamesToScreen;
    }

    void initForServer()
    {
        if (!NetworkManager.Singleton.IsServer)
            return;
        NetworkGamePropertiesStorage.Singleton.myname = NetworkGamePropertiesStorage.Singleton.generateName();
        NetworkGamePropertiesStorage.Singleton.connectedPlayerData.Add(new PlayerData(NetworkManager.Singleton.LocalClientId, NetworkGamePropertiesStorage.Singleton.myname, 0, 0, 0));
        playerNames[(int)(NetworkManager.Singleton.LocalClientId % 8)].text = NetworkGamePropertiesStorage.Singleton.myname;
        playerIcons[(int)(NetworkManager.Singleton.LocalClientId % 8)].SetActive(true);
        StartButton.SetActive(true);

    }

    private void assignNamesToScreen(NetworkListEvent<PlayerData> changeEvent)
    {
        for (int i = 0; i < NetworkGamePropertiesStorage.Singleton.connectedPlayerData.Count; i++)
        {
            playerNames[i].text = NetworkGamePropertiesStorage.Singleton.connectedPlayerData[i].name.ToString();

        }
    }
    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkGamePropertiesStorage.Singleton.connectedPlayerData.Add(new PlayerData(clientId, NetworkGamePropertiesStorage.Singleton.generateName(), 0, 0, 0));
            NetworkGamePropertiesStorage.Singleton.spawnedPlayersIndex[(int)(clientId % 8)] = clientId;
            return;
        }
        playerIcons[(int)(NetworkManager.Singleton.LocalClientId % 8)].SetActive(true);
        ReadyButton.SetActive(true);
    }
    private void OnClientDisconnected(ulong clientId)
    {
        if (NetworkGamePropertiesStorage.Singleton.spawnedPlayers.ContainsKey(clientId))
        {
            //NetworkGamePropertiesStorage.Singleton.spawnedPlayers[clientId].GetComponent<NetworkObject>().Despawn();
            //Destroy(NetworkGamePropertiesStorage.Singleton.spawnedPlayers[clientId]);
            //NetworkGamePropertiesStorage.Singleton.spawnedPlayers.Remove(clientId);
        }
    }

    public void setReady()
    {
        setReady_ServerRpc();
        ReadyButton.SetActive(false);
    }

    [ServerRpc]
    public void setReady_ServerRpc()
    {
        if (!IsServer) return;
        NetworkGamePropertiesStorage.Singleton.readyState += 1;
    }

    public void StartGame()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (NetworkGamePropertiesStorage.Singleton.readyState == NetworkManager.Singleton.ConnectedClients.Count - 1)
        {
            Loader.Singelton.gameObject.SetActive(true);
            NetworkManager.Singleton.SceneManager.LoadScene("MainGameScene", LoadSceneMode.Single);
            Loader.Singelton.setProgress(.1f);
        }
    }

}
public enum playerColors
{

}
