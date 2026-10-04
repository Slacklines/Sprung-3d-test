using UnityEngine;
using Unity.Netcode;

public class PlayerHitboxBehaviour : NetworkBehaviour
{

    private DeathScript ds;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ds = GetComponentInParent<DeathScript>();
    }
}
