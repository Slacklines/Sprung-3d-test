using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class PlayerBounce : NetworkBehaviour
{

    public Transform springOrigin;
    public Transform playerModel;

    public float threshold;
    public float baseJumpForce = 10f;
    public float minimumSpringForce = 10f;
    public float restLength = 4f;
    public float springStrength = 500f;
    public float regDamping = 50f;
    public float scopeDamping = 1f;
    private float damping;
    public LayerMask groundMask;
    public float squashAmount = 0.3f;
    public float maxSpeed;

    //float visualCompression = 0f;
    private PlayerAim playerScript;
    private Rigidbody rb;
    private DeathScript ds;
    private Vector3 originalScale;

    void Start()
    {
        originalScale = playerModel.localScale;

        ds = GetComponentInParent<DeathScript>();
        rb=GetComponentInParent<Rigidbody>();
        playerScript=GetComponentInParent<PlayerAim>();
    }

    void Update()
    {
        damping = Input.GetKey(KeyCode.LeftShift) ? scopeDamping : regDamping;
    }

    void FixedUpdate()
    {
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }

        if (ds.health.Value <= 0) return;

        RaycastHit hit;

        Vector3 springDir = transform.up;

        if (Physics.Raycast(
            springOrigin.position,
            -springDir,
            out hit,
            restLength,
            groundMask))
        {
            float springLength = hit.distance;

            float compression = restLength-springLength;

            if (compression > threshold)
            {

                playerScript.reload();

                float springVelocity = Vector3.Dot(rb.linearVelocity, springDir);

                float springForce = compression * springStrength;

                float dampingForce = springVelocity * damping;

                float totalForce = baseJumpForce + Math.Max(springForce - dampingForce, minimumSpringForce);

                rb.AddForce(springDir * totalForce, ForceMode.Force);

                float compressionRatio = compression / restLength;

                playerModel.localScale = new Vector3 (
                    originalScale.x * (1f + compressionRatio * squashAmount),
                    originalScale.y * (1f - compressionRatio * squashAmount),
                    originalScale.z * (1f + compressionRatio * squashAmount)
                );
            }
        }
        else
        {
            playerModel.localScale = Vector3.Lerp(
                playerModel.localScale,
                originalScale,
                6f * Time.fixedDeltaTime
            );
        }
        
    }
}
