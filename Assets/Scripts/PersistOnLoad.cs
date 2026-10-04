using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PersistOnLoad : NetworkBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

}
