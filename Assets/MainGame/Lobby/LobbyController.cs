using System;
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

    }

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        NetworkGamePropertiesStorage.Singleton.connectedPlayerData.OnListChanged += assignNamesToScreen;

        for (int i = 0; i < NetworkGamePropertiesStorage.Singleton.connectedPlayerData.Count; i++)
        {
            playerNames[i].text = NetworkGamePropertiesStorage.Singleton.connectedPlayerData[i].name.ToString();
        }
    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        NetworkGamePropertiesStorage.Singleton.connectedPlayerData.OnListChanged -= assignNamesToScreen;
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
        if (IsServer)
        {
            NetworkGamePropertiesStorage.Singleton.spawnedPlayersIndex[(int)(clientId % 8)] = clientId;
            string playername = NetworkGamePropertiesStorage.Singleton.generateName();
            NetworkGamePropertiesStorage.Singleton.connectedPlayerData.Add(new PlayerData(clientId, playername, 0, 0, 0));
            setLocalName_ClientRpc(playername, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { clientId } } });
            StartButton.SetActive(true);
        }
        playerIcons[(int)(NetworkManager.LocalClientId % 8)].SetActive(true);
    }

    [ClientRpc]
    public void setLocalName_ClientRpc(string playerName, ClientRpcParams _ = default)
    {
        NetworkGamePropertiesStorage.Singleton.myname = playerName;
        playerNames[NetworkManager.LocalClientId % 8].text = playerName;
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


    [ServerRpc]
    public void setReady_ServerRpc()
    {
        if (IsServer)
            NetworkGamePropertiesStorage.Singleton.readyState.Value += 1;
    }

    public void StartGame()
    {
        if (!IsServer) return;
        Loader.Singelton.setProgress(.1f);
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += NetworkGamePropertiesStorage.Singleton.onSceneLoadComplete;
        NetworkManager.Singleton.SceneManager.LoadScene("MainGameScene", LoadSceneMode.Single);
        StartGame_ClientRpc();
        if (NetworkGamePropertiesStorage.Singleton.readyState.Value == NetworkManager.Singleton.ConnectedClients.Count - 1)
        {
        }
    }
    [ClientRpc]
    public void StartGame_ClientRpc()
    {
        Loader.Singelton.gameObject.SetActive(true);
    }

}
public enum playerColors
{

}
