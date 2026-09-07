using System;
using Unity.Netcode;
using UnityEngine;

public class DeathScript : NetworkBehaviour
{

    public NetworkVariable<bool> isDead = new NetworkVariable<bool>(false,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Owner);
    public GameObject PlayerModelGroup;
    public CanvasGroup deathScreen;
    public override void OnNetworkSpawn()
    {
        isDead.OnValueChanged += OnDeathChanged;
    }
    void Start()
    {
        deathScreen = GameObject.FindWithTag("DeathScreen")?.GetComponent<CanvasGroup>();
    }

    void Update()
    {
        if (isDead.Value)
        {
            transform.root.position = new Vector3 (100,100,100);
        }
        if (isDead.Value && Input.GetKeyDown(KeyCode.R))
        {
            respawn();
        }
    }

    void OnDeathChanged(bool oldValue, bool newValue)
    {
        Debug.Log(newValue);
        if (!IsOwner) return;

        if (!deathScreen) deathScreen = GameObject.FindWithTag("DeathScreen")?.GetComponent<CanvasGroup>();


        if(newValue) {
            deathScreen.alpha = 1;
            PlayerModelGroup.SetActive(false);
        }
        else
        {
            deathScreen.alpha = 0;
            PlayerModelGroup.SetActive(true);
        }
    }

    public void respawn()
    {
        isDead.Value = false;

        transform.position = Vector3.zero;

        GetComponentInParent<Rigidbody>().linearVelocity = Vector3.zero;
    }

}
