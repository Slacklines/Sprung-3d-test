using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : NetworkBehaviour
{
    [SerializeField] public Dictionary<ulong, int> playerList = new ();
    public string roomCode = "XXXXXX";
    public static PlayerManager instance;

    void Awake()
    {
        instance = this;
    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
            
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    public void OnClientDisconnected(ulong clientId)
    {
        FeedManager feedManager = FeedManager.instance;
        if (feedManager) feedManager.AddMessage("Player " + playerList[clientId] + " left!");
        RemovePlayer(clientId);
        Debug.Log("Player " + playerList[clientId] + " left!");
    }

    public void OnClientConnected(ulong clientId)
    {
        AddPlayer(clientId);
        FeedManager feedManager = FeedManager.instance;

        if (feedManager) feedManager.AddMessage("Player " + playerList[clientId] + " joined!");
        Debug.Log("Player " + playerList[clientId] + " joined!");
    }

    public void AddPlayer(ulong clientId)
    {
        if (playerList.ContainsKey(clientId)) return;
        int i = 1;
        while (playerList.ContainsValue(i)) i++;

        playerList[clientId] = i;
    }

    public void RemovePlayer(ulong clientId)
    {
        playerList.Remove(clientId);
    }
}
