using Unity.Netcode;
using UnityEngine;

public class DeathUI : MonoBehaviour
{
    public void RespawnRequest()
    {
        var player = NetworkManager.Singleton.LocalClient.PlayerObject;

        player.GetComponent<DeathScript>().respawn();
    }
}
