using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
using Unity.Netcode;

public class MenuManager : MonoBehaviour
{
    public void HostGame()
    {
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("Map1", LoadSceneMode.Single);
    }

    public void JoinLAN()
    {
        NetworkManager.Singleton.StartClient();
    }

}
