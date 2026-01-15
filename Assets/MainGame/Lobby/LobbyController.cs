using System;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;


public class LobbyController : NetworkBehaviour
{
    [SerializeField] TMP_Text[] playerNames;
    [SerializeField] GameObject[] playerIcons;
    [SerializeField] GameObject StartButton;
    [SerializeField] GameObject ReadyButton;

    private ulong[] spawnedPlayersIndex = new ulong[8];
    private Dictionary<ulong, GameObject> spawnedPlayers = new Dictionary<ulong, GameObject>();
    private NetworkList<PlayerNameData> spawnedPlayersNames;
    private NetworkVariable<int> readyState;
    private string[] names = { "Avocado", "Potato", "Tomato", "Radish", "Carrot", "Bamboo", "Bean", "Cabbage" };
    private string[] namesPrefix = { "Alpha", "Bravo", "Charlie", "Delta", "Echo", "Foxtrot", "Golf", "Hotel" };

    private string myname;
    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        spawnedPlayersNames.OnListChanged += assignNamesToScreen;
        if (NetworkManager.Singleton.IsServer)
        {
            spawnedPlayersNames = new NetworkList<PlayerNameData>();
            initForServer();
        }
        else
            initForMe();
    }

    private void OnDisable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    string generateName()
    {
        return namesPrefix[UnityEngine.Random.Range(0, 9)] + "." + names[UnityEngine.Random.Range(0, 9)];
    }

    void initForMe()
    {
        playerIcons[(int)(NetworkManager.Singleton.LocalClientId % 8)].SetActive(true);
        ReadyButton.SetActive(true);
    }

    void initForServer()
    {
        if (!NetworkManager.Singleton.IsServer)
            return;
        myname = generateName();
        spawnedPlayersNames.Add(new PlayerNameData(NetworkManager.Singleton.LocalClientId, myname));
        playerNames[(int)(NetworkManager.Singleton.LocalClientId % 8)].text = myname;
        playerIcons[(int)(NetworkManager.Singleton.LocalClientId % 8)].SetActive(true);
        StartButton.SetActive(true);

    }

    private void assignNamesToScreen(NetworkListEvent<PlayerNameData> changeEvent)
    {
        playerNames[(int)(changeEvent.Value.clientId % 8)].text = changeEvent.Value.name.ToString();
    }
    private void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer)
            return;
        spawnedPlayersNames.Add(new PlayerNameData(clientId, generateName()));
        spawnedPlayersIndex[(int)(clientId % 8)] = clientId;
    }
    private void OnClientDisconnected(ulong clientId)
    {
        if (spawnedPlayers.ContainsKey(clientId))
        {
            spawnedPlayers[clientId].GetComponent<NetworkObject>().Despawn();
            Destroy(spawnedPlayers[clientId]);
            spawnedPlayers.Remove(clientId);
        }
    }

    public void setReady()
    {
        ReadyButton.SetActive(false);
        readyState.Value++;
    }

    public void StartGame()
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (readyState.Value == NetworkManager.Singleton.ConnectedClients.Count - 1)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("MainGame", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

}
public enum playerColors
{

}

public struct PlayerReadyData : INetworkSerializable, IEquatable<PlayerNameData>
{
    public ulong clientId;
    public bool ready;

    public PlayerReadyData(ulong clientId, bool ready)
    {
        this.clientId = clientId;
        this.ready = ready;
    }
    public bool Equals(PlayerNameData other)
    {
        return clientId == other.clientId;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref ready);
    }
}

public struct PlayerNameData : INetworkSerializable, IEquatable<PlayerNameData>
{
    public ulong clientId;
    public FixedString32Bytes name;

    public PlayerNameData(ulong clientId, string name)
    {
        this.clientId = clientId;
        this.name = name;
    }
    public bool Equals(PlayerNameData other)
    {
        return clientId == other.clientId;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref name);
    }
}