using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class NetworkGamePropertiesStorage : NetworkBehaviour
{
    public static NetworkGamePropertiesStorage Singleton;
    public GameObject playerPrefab;
    public ulong[] spawnedPlayersIndex = new ulong[8];
    public Dictionary<ulong, GameObject> spawnedPlayers = new Dictionary<ulong, GameObject>();
    public NetworkList<PlayerData> connectedPlayerData;
    public NetworkVariable<int> readyState;
    public NetworkVariable<int> WorldGenerationSeed;
    public BaseResource[,] resourcesInGame;
    public string myname;

    private string[] names = { "Avocado", "Potato", "Tomato", "Radish", "Carrot", "Bamboo", "Bean", "Cabbage" };
    private string[] namesPrefix = { "Alpha", "Bravo", "Charlie", "Delta", "Echo", "Foxtrot", "Golf", "Hotel" };
    void Awake()
    {
        if (Singleton == null) Singleton = this;
        DontDestroyOnLoad(gameObject);
        connectedPlayerData = new NetworkList<PlayerData>();
    }

    public string generateName()
    {
        return namesPrefix[UnityEngine.Random.Range(0, 8)] + "." + names[UnityEngine.Random.Range(0, 8)];
    }
}


public struct PlayerReadyData : INetworkSerializable, IEquatable<PlayerReadyData>
{
    public ulong clientId;
    public bool ready;

    public PlayerReadyData(ulong clientId, bool ready)
    {
        this.clientId = clientId;
        this.ready = ready;
    }
    public bool Equals(PlayerReadyData other)
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

public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
{
    public ulong clientId;
    public FixedString32Bytes name;

    public int headCustomisation;
    public int bodyCustomisation;
    public int shoeCustomisation;

    public PlayerData(ulong clientId, string name, int headCustomisation, int bodyCustomisation, int shoeCustomisation)
    {
        this.clientId = clientId;
        this.name = name;
        this.headCustomisation = headCustomisation;
        this.bodyCustomisation = bodyCustomisation;
        this.shoeCustomisation = shoeCustomisation;
    }
    public bool Equals(PlayerData other)
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