using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Managers.Network;
using _Scripts.NetworkContainter;
using _Scripts.Scriptable_Objects;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMultiplayerManager : PersistentSingletonNetworkBehavior<GameMultiplayerManager> 
{


    public const int MAX_PLAYER_AMOUNT = 4;
    private const string PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER = "PlayerNameMultiplayer";

    
    public bool PlayMultiplayer = true;


    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;
    public event EventHandler OnPlayerContainerNetworkListChanged;
    
    [SerializeField] private List<ChampionDescription> _playerChampionList;


    private NetworkList<PlayerContainer> _playerContainerNetworkList;
    private string _playerName;


    protected override void Awake() {
        base.Awake();
        
        _playerName = PlayerPrefs.GetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, "PlayerName" + UnityEngine.Random.Range(100, 1000));

        _playerContainerNetworkList = new NetworkList<PlayerContainer>();
        _playerContainerNetworkList.OnListChanged += PlayerContainerNetworkList_OnListChanged;
    }

    private void Start() {
        if (!PlayMultiplayer) {
            // Singleplayer
            StartHost();
            AssetNetworkSceneManager.LoadNetworkScene(AssetSceneManager.AssetScene.GameScene.ToString());
        }
    }

    public string GetPlayerName() {
        return _playerName;
    }

    public void SetPlayerName(string playerName) {
        this._playerName = playerName;

        PlayerPrefs.SetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, playerName);
    }

    private void PlayerContainerNetworkList_OnListChanged(NetworkListEvent<PlayerContainer> changeEvent) {
        OnPlayerContainerNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void StartHost() {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback;
        NetworkManager.Singleton.StartHost();
    }

    private void NetworkManager_Server_OnClientDisconnectCallback(ulong clientId) {
        for (int i = 0; i < _playerContainerNetworkList.Count; i++) {
            PlayerContainer playerContainer = _playerContainerNetworkList[i];
            if (playerContainer.ClientID == clientId) {
                // Disconnected!
                _playerContainerNetworkList.RemoveAt(i);
            }
        }
    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientId) {
        _playerContainerNetworkList.Add(new PlayerContainer {
            ClientID = clientId,
            ChampionID = GetFirstUnusedChampionId(),
        });
        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse) {
        if (SceneManager.GetActiveScene().name != AssetSceneManager.AssetScene.CharacterSelectScene.ToString()) {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game has already started";
            return;
        }

        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_AMOUNT) {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game is full";
            return;
        }

        connectionApprovalResponse.Approved = true;
    }

    public void StartClient() {
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);

        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Client_OnClientConnectedCallback;
        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_Client_OnClientConnectedCallback(ulong clientId) {
        SetPlayerNameServerRpc(GetPlayerName());
        SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerNameServerRpc(string playerName, ServerRpcParams serverRpcParams = default) {
        int playerContainerIndex = GetPlayerContainerIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerContainer playerContainer = _playerContainerNetworkList[playerContainerIndex];

        playerContainer.PlayerName = playerName;

        _playerContainerNetworkList[playerContainerIndex] = playerContainer;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerIdServerRpc(string playerId, ServerRpcParams serverRpcParams = default) {
        int playerContainerIndex = GetPlayerContainerIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerContainer playerContainer = _playerContainerNetworkList[playerContainerIndex];

        playerContainer.PlayerID = playerId;

        _playerContainerNetworkList[playerContainerIndex] = playerContainer;
    }

    private void NetworkManager_Client_OnClientDisconnectCallback(ulong clientId) {
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
    }
    
    public bool IsPlayerIndexConnected(int playerIndex) {
        return playerIndex < _playerContainerNetworkList.Count;
    }

    public int GetPlayerContainerIndexFromClientId(ulong clientId) {
        for (int i=0; i< _playerContainerNetworkList.Count; i++) {
            if (_playerContainerNetworkList[i].ClientID == clientId) {
                return i;
            }
        }
        return -1;
    }

    public PlayerContainer[] GetAllPlayerContainer()
    {
        PlayerContainer [] playerContainers = new PlayerContainer[_playerContainerNetworkList.Count];

        for (var index = 0; index < _playerContainerNetworkList.Count; index++)
        {
            var playerContainer = _playerContainerNetworkList[index];
            playerContainers[index] = playerContainer;
        }

        return playerContainers;
    }
    
    public PlayerContainer GetPlayerContainerFromClientId(ulong clientId) {
        foreach (PlayerContainer playerContainer in _playerContainerNetworkList) {
            if (playerContainer.ClientID == clientId) {
                return playerContainer;
            }
        }
        return default;
    }

    public PlayerContainer GetPlayerContainer() {
        return GetPlayerContainerFromClientId(NetworkManager.Singleton.LocalClientId);
    }

    public PlayerContainer GetPlayerContainerFromPlayerIndex(int playerIndex) {
        return _playerContainerNetworkList[playerIndex];
    }

    public ChampionDescription GetPlayerChampion(int championId) {
        return _playerChampionList[championId];
    }

    public void ChangePlayerChampion(int championId) {
        ChangePlayerChampionServerRpc(championId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangePlayerChampionServerRpc(int championId, ServerRpcParams serverRpcParams = default) {
        if (!IsChampionAvailable(championId)) {
            // Champion not available
            
            
            return;
        }

        int playerContainerIndex = GetPlayerContainerIndexFromClientId(serverRpcParams.Receive.SenderClientId);

        PlayerContainer playerContainer = _playerContainerNetworkList[playerContainerIndex];

        playerContainer.ChampionID = championId;

        _playerContainerNetworkList[playerContainerIndex] = playerContainer;
    }

    private bool IsChampionAvailable(int championId) {
        foreach (PlayerContainer playerContainer in _playerContainerNetworkList) {
            if (playerContainer.ChampionID == championId) {
                // Already in use
                return false;
            }
        }
        return true;
    }

    private int GetFirstUnusedChampionId() {
        for (int i = 0; i<_playerChampionList.Count; i++) {
            if (IsChampionAvailable(i)) {
                return i;
            }
        }
        return -1;
    }

    public void KickPlayer(ulong clientId) {
        NetworkManager.Singleton.DisconnectClient(clientId);
        NetworkManager_Server_OnClientDisconnectCallback(clientId);
    }

}