using System.Collections.Generic;
using System.Threading;
using Unity.Netcode;
using UnityEngine;

public class serverReadyManager : NetworkBehaviour
{

    private HashSet<ulong> readyPlayers = new();
    private bool allReady = false;
    private float timer = 6f;
    private int currentTime;

    public static serverReadyManager Instance;

    private void Awake()
    {
        Instance = this;
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

    public void OnClientConnected(ulong clientId)
    {
        CheckAllReady();
    }
    public void OnClientDisconnected(ulong clientId)
    {
        readyPlayers.Remove(clientId);
        CheckAllReady();
    }

    public void playerEnterZone(ulong clientId)
    {
        readyPlayers.Add(clientId);
        CheckAllReady();
    }

    public void playerLeftZone(ulong clientId)
    {
        readyPlayers.Remove(clientId);
        CheckAllReady();
    }

    private void CheckAllReady()
    {
        if (readyPlayers.Count == NetworkManager.Singleton.ConnectedClients.Count)
        {
            allReady = true;
        }
        else
        {
            allReady = false;
        }
    }

    void Update()
    {
        if (!IsServer) return;
        if (allReady)
        {
            if (timer <= 0)
            {
                //FeedManager.instance.AddMessage("Starting game");
                NetworkManager.Singleton.SceneManager.LoadScene("Map1", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
            else
            {
                if (timer <= currentTime)
                {
                    if (currentTime == 1)
                    {
                        FeedManager.instance.AddMessage("Game is starting...");
                    }
                    else
                    {
                        FeedManager.instance.AddMessage("Game starting in " + (currentTime-1));                     
                    }
                    currentTime--;
                }
                timer -= Time.deltaTime;
            }
        }
        else
        {
            timer = 6f;
            currentTime = 6;
        }

    }
}
