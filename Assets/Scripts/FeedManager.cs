using Unity.Netcode;
using UnityEditor.VersionControl;
using UnityEngine;

public class FeedManager : NetworkBehaviour
{

    public static FeedManager instance;
    public GameObject messagePrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        instance = this;
    }

    public void AddMessage(string message)
    {
        AddMessageClientRpc(message);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void AddMessageClientRpc(string message)
    {
        GameObject newMessage = Instantiate(messagePrefab,transform);

        newMessage.GetComponent<FeedMessageBehaviour>().Initialize(message);       
    }
}
