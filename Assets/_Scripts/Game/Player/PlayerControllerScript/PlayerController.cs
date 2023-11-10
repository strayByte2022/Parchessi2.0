using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public PlayerTurnController PlayerTurnController { get; internal set; }
    public PlayerActionController PlayerActionController { get; internal set; }
    public PlayerResourceController PlayerResourceController { get; internal set; }


    private void Awake()
    {
        PlayerTurnController = GetComponent<PlayerTurnController>();
        PlayerActionController = GetComponent<PlayerActionController>();
        PlayerResourceController = GetComponent<PlayerResourceController>();
    }

    public void Initialize()
    {
        PlayerTurnController = GetComponent<PlayerTurnController>();
        PlayerActionController = GetComponent<PlayerActionController>();
        PlayerResourceController = GetComponent<PlayerResourceController>();
    }
    
    
}