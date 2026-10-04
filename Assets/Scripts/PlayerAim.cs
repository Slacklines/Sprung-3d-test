using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Netcode;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using Unity.VisualScripting;
using System.Security.Cryptography;
using System.Net.Mail;
using System;

public class PlayerAim : NetworkBehaviour
{
    //SHOOTING VARS
    public struct ShootRayData
    {
        public Vector3 Start;
        public Vector3 End;
        public GameObject HitObj;
    }

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
    private DeathScript ds;

    private RectTransform ammoBar;
    private RectTransform healthBar;

    public NetworkVariable<int> playerId = new();
    public List<Renderer> playerColorRenderers;
    public List<Material> playerColors;

    public override void OnNetworkSpawn()
    {
        playerCamera.enabled = IsOwner;
        playerId.OnValueChanged += playerIdChanged;
        if (IsServer)
        {
            PlayerManager.instance.AddPlayer(OwnerClientId);
            playerId.Value = PlayerManager.instance.playerList[OwnerClientId]; 
            playerIdChanged(0, playerId.Value);
        } 
        playerIdChanged(0, playerId.Value);
    }

    public override void OnNetworkDespawn()
    {
        playerId.OnValueChanged -= playerIdChanged;
    }

    public void playerIdChanged(int oldVal, int newVal)
    {
        Debug.Log("Player Id Changed");
        if (newVal <= 0)
        {
            Debug.Log("New val not created yet");
            return;
        }
        foreach (var renderer in playerColorRenderers)
        {
            renderer.material = playerColors[newVal-1];
        }
    }

    void Start()
    {
        //if (!IsOwner) return;

        ds = GetComponent<DeathScript>();

        rb = GetComponent<Rigidbody>();

        if (!IsOwner) return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!IsOwner) return;

        //UI BARS

        if (ammoBar == null){
            ammoBar = GameObject.FindGameObjectWithTag("Ammo")?.GetComponent<RectTransform>();
        }
        else
        {
            float ammoScale = 1f*bullets/maxBullets;
            ammoBar.localScale = new Vector3 (
                1f,
                Mathf.Lerp(ammoBar.localScale.y, ammoScale, Time.deltaTime * 5f),
                1f
            );
        }

        if (healthBar == null){
            healthBar = GameObject.FindGameObjectWithTag("Health")?.GetComponent<RectTransform>();
        }
        else
        {
            float healthScale = ds.health.Value/100f;
            healthBar.localScale = new Vector3 (
                1f,
                Mathf.Lerp(healthBar.localScale.y, healthScale, Time.deltaTime * 7f),
                1f
            );
        }

        //INPUT HANDLING

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

        if (ds.health.Value <= 0){
            playerCamera.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
            return;
        }
        else
        {
            playerCamera.transform.localEulerAngles = new Vector3 (90, 0, 0);
        }

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        Scoped = Input.GetKey(KeyCode.LeftShift);

        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) && bullets > 0)
        {
            shoot();
        }

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
        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.forward;

        bullets--;

        ShootRayData rayData = BulletRaycast(origin, direction);
        RenderTracer(rayData.Start, rayData.End);

        ShootServerRpc(origin, direction, 25);
    }

    [Rpc(SendTo.Server)]
    void ShootServerRpc(Vector3 origin, Vector3 direction, int damage, RpcParams rpcParams = default)
    {

        //Calculate bullet and apply damage

        ShootRayData rayData = BulletRaycast(origin, direction);

        NetworkObject netObj = rayData.HitObj?.GetComponentInParent<NetworkObject>();
        if(netObj != null)
        {
            DeathScript targetDs = netObj.GetComponentInParent<DeathScript>();
            if (targetDs) targetDs.DamageRequestRpc(damage);
        }

        //Gather recipients excludeing the original client

        ulong shooterId = rpcParams.Receive.SenderClientId;

        List<ulong> recipients = new();

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (clientId != shooterId) recipients.Add(clientId);
        }

        ShowRayClientRpc(
            rayData.Start, 
            rayData.End,
            RpcTarget.Group(recipients, RpcTargetUse.Temp)
        );
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void ShowRayClientRpc(Vector3 start, Vector3 end, RpcParams rpcParams = default)
    {
        RenderTracer(start, end);
    }

    ShootRayData BulletRaycast(Vector3 origin, Vector3 direction)
    {
        ShootRayData returnData = new ();
        returnData.Start = origin;
        returnData.End = origin+direction*100f;

        RaycastHit[] hits = Physics.RaycastAll(origin, direction, 100f, ~0, QueryTriggerInteraction.Collide);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if(hit.collider.transform.root == transform.root) continue;

            returnData.HitObj = hit.collider.gameObject;
            returnData.End = hit.point;
            break;
        }

        return returnData;
    }

    void RenderTracer(Vector3 start, Vector3 end)
    {
        GameObject curTracer = Instantiate(tracer);

        LineRenderer lr = curTracer.GetComponent<LineRenderer>();

        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        Destroy(curTracer, 0.1f);
    }
}
