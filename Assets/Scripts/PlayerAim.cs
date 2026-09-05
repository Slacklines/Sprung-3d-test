using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Netcode;

public class PlayerAim : NetworkBehaviour
{
    public float viewLimit = 80f;
    public float mouseSensitivity;

    public GameObject tracer;

    [SerializeField] Camera playerCamera;
    float yaw;
    float pitch;

    Rigidbody rb;
  
    public override void OnNetworkSpawn()
    {
        playerCamera.enabled = IsOwner;
    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            shoot();
        }

        if(transform.position.y < -20)
        {
            transform.position = new Vector3 (0,0,0);
            rb.linearVelocity = Vector3.zero;
        }
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;

        //pitch= Mathf.Clamp(pitch, -viewLimit, viewLimit);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void shoot()
    {
        rb.AddForce(transform.up*15f, ForceMode.Impulse);
        Vector3 orgin = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.forward;

        ShootServerRpc(orgin, direction);
    }

    [Rpc(SendTo.Server)]
    void ShootServerRpc(Vector3 origin, Vector3 direction)
    {
        Vector3 end = origin+direction*100f;

        RaycastHit[] hits = Physics.RaycastAll(origin, direction, 100f);

        foreach (RaycastHit hit in hits)
        {
            NetworkObject netObj = hit.collider.GetComponentInParent<NetworkObject>();

            if(netObj == NetworkObject) continue;

            end = hit.point;

            if(netObj != null)
            {
                netObj.transform.position = Vector3.zero;
                netObj.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
                break;
            }
            break;
        }
        ShowRayClientRpc(origin, end);
    }

    [Rpc(SendTo.Everyone)]
    void ShowRayClientRpc(Vector3 start, Vector3 end)
    {
        GameObject curTracer = Instantiate(tracer);

        LineRenderer lr = curTracer.GetComponent<LineRenderer>();

        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        Destroy(curTracer, 0.5f);
    }
}
