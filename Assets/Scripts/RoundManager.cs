using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class RoundManager : NetworkBehaviour
{

    private HashSet<ulong> alivePlayers = new();

    public static RoundManager instance;
    private ulong winner;

    private float winTime = 10f;
    private float timer;
    public bool roundActive = true;

    public Transform[] spawnpoints;
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (!IsServer) return;
        timer=winTime;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClients.Keys)
        {
            alivePlayers.Add(clientId);
        }

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var player = client.PlayerObject;
            var playerDeathScript = player.GetComponent<DeathScript>();

            if (playerDeathScript) playerDeathScript.ResetPositionRpc();
        }

    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    public void OnClientDisconnected(ulong clientId)
    {
        alivePlayers.Remove(clientId);
    }

    public void PlayerDie(ulong clientId)
    {
        alivePlayers.Remove(clientId);
    }

    public void PlayerRespawn(ulong clientId)
    {
        alivePlayers.Add(clientId);
    }

    private void ResetAllPlayers()
    {
        foreach(var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            NetworkObject playerObject = client.PlayerObject;

            if (playerObject == null) continue;

            playerObject.GetComponent<DeathScript>().ApplyReset();
        }
    }

    private void Update()
    {
        if (!IsServer) return;
        if (alivePlayers.Count <= 1)
        {
            if (timer <= 0)
            {
                ResetAllPlayers();
                NetworkManager.Singleton.SceneManager.LoadScene("Map1", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
            else
            {
                if (roundActive == true)
                {
                    if (alivePlayers.Count == 1)
                    {
                        winner = alivePlayers.First();
                        FeedManager.instance.AddMessage("Player " + PlayerManager.instance.playerList[winner] + " wins"); 
                    }
                    else
                    {
                        FeedManager.instance.AddMessage("Nobody wins");
                    }
                    roundActive=false;
                }
                timer -= Time.deltaTime;
            }
        }
        else
        {
            roundActive=true;
            timer=winTime;
        }
    }
}

