using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class PlayerBounce : NetworkBehaviour
{

    public float power = 3f;
    private PlayerAim playerScript;
    public Rigidbody rb;

    void Start()
    {
        rb=GetComponentInParent<Rigidbody>();
        playerScript=GetComponentInParent<PlayerAim>();
    }

    void OnTriggerStay(Collider other)
    {
        rb.AddForce(transform.up*power, ForceMode.Impulse);
        playerScript.reload();
    }

}
