using UnityEngine;
using Unity.Netcode;

public class readyScript : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent(out NetworkObject netObj))
        {
            serverReadyManager.Instance.playerEnterZone(netObj.OwnerClientId);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent(out NetworkObject netObj))
        {
            serverReadyManager.Instance.playerLeftZone(netObj.OwnerClientId);
        }
    }
}
