using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class PlayerBounce : NetworkBehaviour
{

    public float power = 3f;
    public float recoil = 20f;
    public int bullets = 3;
    public Rigidbody rb;
    void OnTriggerStay(Collider other)
    {
        if (!IsOwner) return;
        rb.AddForce(transform.up*power, ForceMode.Impulse);
    }
}
