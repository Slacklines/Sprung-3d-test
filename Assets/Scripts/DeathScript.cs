using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScript : NetworkBehaviour
{

    public bool IsTouching;
    public Transform DamageOrigin;
    public NetworkVariable<float> health = new NetworkVariable<float>(100,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server);
    public GameObject PlayerModelGroup;
    public CanvasGroup deathScreen;
    private Rigidbody rb;
    private Camera playerCamera;

    //private FeedManager fm;
    private PlayerManager pm;
    private RoundManager rm;
    public bool inLobby = true;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCamera = GetComponentInChildren<Camera>();
        deathScreen = GameObject.FindWithTag("DeathScreen")?.GetComponent<CanvasGroup>();
        pm = PlayerManager.instance;
        if (IsOwner) SetLayer(PlayerModelGroup, 7);
    }

    void Update()
    {
        if (IsServer)
        {
            IsTouching = Physics.CheckSphere(DamageOrigin.position, .6f, 1 << 6);

            if (health.Value > 0 && IsTouching)
            {
                ApplyDamage(100f * Time.deltaTime);
            }
        }

        if(!IsOwner) return;

        if (SceneManager.GetActiveScene().name == "Lobby" && health.Value <= 0  && Input.GetKeyDown(KeyCode.R))
        {
            ResetRequestRpc();
        }
    }

    void SetLayer(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            child.gameObject.layer=layer;
        }
    }

    [Rpc(SendTo.Owner)]
    public void DetachPlayerRpc()
    {
        //Detach Camera
        playerCamera.transform.SetParent(null, true);
        //Apply random spin
        rb.freezeRotation = false;
        rb.angularVelocity = UnityEngine.Random.onUnitSphere * rb.linearVelocity.magnitude * 2f;
        //Make model visible locally
        SetLayer(PlayerModelGroup, 0);
    }

    [Rpc(SendTo.Owner)]
    public void ResetPlayerRpc()
    {
        //Lock rotation
        rb.freezeRotation = true;
        //Reattach Camera
        playerCamera.transform.SetParent(transform, false);
        playerCamera.transform.localPosition = new Vector3 (0, 0.9f, 0);
        playerCamera.transform.localEulerAngles = new Vector3 (90, 0, 0);
        //Hide model locally
        SetLayer(PlayerModelGroup, 7);
        //Reset
        ResetPosition();
        transform.eulerAngles = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    [Rpc(SendTo.Server)]
    public void ResetRequestRpc()
    {
        ApplyReset();
    }

    public void ApplyReset()
    {
        health.Value = 100;
        ResetPlayerRpc();
        RoundManager rm = RoundManager.instance;
        if (rm) rm.PlayerRespawn(OwnerClientId);
    }

    [Rpc(SendTo.Server)]
    public void DamageRequestRpc(float damage)
    {
        ApplyDamage(damage);
    }

    public void ApplyDamage(float damage)
    {
        if (health.Value<=0) return;
        health.Value = Math.Max(health.Value - damage, 0);
        if (health.Value <= 0)
        {
            DetachPlayerRpc();
            FeedManager fm = FeedManager.instance;
            if (fm) fm.AddMessage("Player " + pm.playerList[OwnerClientId] + " died");

            RoundManager rm = RoundManager.instance;
            if (rm) rm.PlayerDie(OwnerClientId);
        }
    }

    [Rpc(SendTo.Owner)]
    public void ResetPositionRpc()
    {
        ResetPosition();
    }

    public void ResetPosition()
    {
        int playerId = GetComponent<PlayerAim>().playerId.Value;
        RoundManager rm = RoundManager.instance;
        if (rm){
            Debug.Log("Spawning");
            Transform spawnpoint = RoundManager.instance.spawnpoints[playerId-1];
            transform.root.position = spawnpoint.position;
        }
        else
        {
            transform.root.position = Vector3.zero;
        }
    }

}
