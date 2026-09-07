using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Netcode;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using Unity.VisualScripting;

public class PlayerAim : NetworkBehaviour
{
    //SHOOTING VARS
    public GameObject tracer;
    public int maxBullets = 5;
    public float recoil = 20f;
    private int bullets = 0;

    //AIM VARS
    public float FOVBase = 100f;
    public float FOVScoped = 80f;
    public float mouseSensitivity;
    //public float mouseSensitivityScoped;
    [SerializeField] Camera playerCamera;
    //private float mouseSensitivity;

    private bool Scoped;
    private float yaw;
    private float pitch;

    //MISC VARS
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

        //INPUT HANDLING

        Scoped = Input.GetKey(KeyCode.LeftShift);

        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) && bullets > 0)
        {
            shoot();
        }

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        //ROTATION AND FOV

        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView,
            Scoped ? FOVScoped : FOVBase,
            10f * Time.deltaTime
        );

        yaw += mouseX;
        pitch -= mouseY;

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        //OUT OF BOUNDS CHECK

        if(transform.position.y < -20)
        {
            transform.position = new Vector3 (0,0,0);
            rb.linearVelocity = Vector3.zero;
        }
    }

    //SHOOTING FUNCTIONS
    public void reload()
    {
        bullets=maxBullets;
    }
    void shoot()
    {
        if(!Scoped) rb.AddForce(transform.up*recoil, ForceMode.Impulse);
        Vector3 orgin = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.forward;

        bullets--;

        ShootServerRpc(orgin, direction);
    }

    [Rpc(SendTo.Server)]
    void ShootServerRpc(Vector3 origin, Vector3 direction)
    {
        Vector3 end = origin+direction*100f;

        RaycastHit[] hits = Physics.RaycastAll(origin, direction, 100f, ~0, QueryTriggerInteraction.Collide);

        Debug.Log(hits.Length);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        Debug.Log(hits.Length);

        foreach (RaycastHit hit in hits)
        {
            Debug.Log(hit.collider);
  
            NetworkObject netObj = hit.collider.GetComponentInParent<NetworkObject>();

            if(netObj == NetworkObject) continue;

            end = hit.point;

            if(netObj != null)
            {
                netObj.transform.position = Vector3.zero;
                netObj.GetComponent<DeathScript>().isDead.Value = true;
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
