using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Managers.Network;
using Unity.Netcode;
using UnityEngine;
using UnityUtilities;

public class CharacterSelectReadyManager : SingletonMonoBehaviour<CharacterSelectReadyManager> 
{
    public event EventHandler OnReadyChanged;
    private Dictionary<ulong, bool> _playerReadyDictionary;


    private void Awake() {

        _playerReadyDictionary = new Dictionary<ulong, bool>();
    }


    public void SetPlayerReady() {
        SetPlayerReadyServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ulong senderClientId, ServerRpcParams serverRpcParams = default) {
        //SetPlayerReadyClientRpc(serverRpcParams.Receive.SenderClientId);
        SetPlayerReadyClientRpc(senderClientId);
        
        //_playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;
        _playerReadyDictionary[senderClientId] = true;
        
        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds) {
            if (!_playerReadyDictionary.ContainsKey(clientId) || !_playerReadyDictionary[clientId]) {
                // This player is NOT ready
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady) {
            GameLobbyManager.Instance.DeleteLobby();
            AssetNetworkSceneManager.LoadNetworkScene(AssetSceneManager.AssetScene.GameScene.ToString());
        }
    }

    [ClientRpc]
    private void SetPlayerReadyClientRpc(ulong clientId) {
        _playerReadyDictionary[clientId] = true;

        OnReadyChanged?.Invoke(this, EventArgs.Empty);
    }


    public bool IsPlayerReady(ulong clientId) {
        return _playerReadyDictionary.ContainsKey(clientId) && _playerReadyDictionary[clientId];
    }

}