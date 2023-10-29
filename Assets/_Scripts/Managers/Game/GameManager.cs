using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Managers.Network;
using _Scripts.NetworkContainter;
using _Scripts.Scriptable_Objects;
using QFSW.QC;
using Shun_Unity_Editor;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityUtilities;


public class GameManager : SingletonNetworkBehavior<GameManager>
{
    public enum GameState
    {
        NetworkSetup,
        GameSetup,
        GamePlay,
        GamePause,
        GameEnd
    }

    [SerializeField] public List<PlayerController> PlayerControllers { get; private set; } = new();
    [SerializeField, ShowImmutable] private GameState _gameState = GameState.NetworkSetup;
    
    private readonly NetworkVariable<int> _playerIdTurn = new NetworkVariable<int>(0);


    public PlayerController ClientOwnerPlayerController;
    public Action OnNetworkSetUp { get; set; }
    public Action OnClientPlayerControllerSetUp { get; set; }
    public Action OnGameStart { get; set; }
    public Action OnGameEnd { get; set; }
    public Action OnGamePause { get; set; }
    public Action OnGameResume { get; set; }
    public Action OnGameQuit { get; set; }

    public Action<PlayerController> OnPlayerTurnStart { get; set; }

    public Action<PlayerTurnController.PlayerPhase, PlayerTurnController.PlayerPhase, PlayerController>
        OnPlayerPhaseChanged { get; set; }

    public Action<PlayerController> OnPlayerTurnEnd { get; set; }

    [SerializeField] private List<DiceDescription> _incomeDiceDescriptions = new();
    [SerializeField] private List<CardDescription> _deckCardDescriptions = new();
    [SerializeField] private int _victoryPointRequirement = 4;

    private Dictionary<PlayerController, bool> _serverPlayerControllerReady = new();
    
    public void Start()
    {
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        //Invoke(nameof(CreatePlayerController) , 1f);

        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    [Command]
    public void CreatePlayerController()
    {
        CreatePlayerControllerServerRPC(NetworkManager.LocalClientId);
    }
    

    private void SceneManager_OnLoadEventCompleted(string sceneName,
        UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted,
        List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            CreatePlayerControllerServerRPC(clientId);
        }

    }

    

    [ServerRpc(RequireOwnership = false)]
    private void CreatePlayerControllerServerRPC(ulong clientId)
    {
        var playerController = Instantiate(GameResourceManager.Instance.PlayerControllerPrefab);
        var networkObject = playerController.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId, true);
        CreatePlayerControllerClientRPC(networkObject.NetworkObjectId);
    }

    [ClientRpc]
    private void CreatePlayerControllerClientRPC(ulong objectId)
    {
        var networkObject = NetworkManager.SpawnManager.SpawnedObjects[objectId];
        var playerController = networkObject.GetComponent<PlayerController>();
        playerController.Initialize();
        StartPlayerController(playerController);

        if (GameMultiplayerManager.Instance != null && PlayerControllers.Count == GameMultiplayerManager.Instance.GetAllPlayerContainer().Length)
        {
            SetReadyPlayerControllerServerRPC(NetworkManager.LocalClientId);
        }
    }

    

    [ServerRpc(RequireOwnership = false)]
    private void SetReadyPlayerControllerServerRPC(ulong clientId)
    {
        _serverPlayerControllerReady[GetPlayerController(clientId)] = true;
        
        if (_serverPlayerControllerReady.Count == NetworkManager.Singleton.ConnectedClients.Count)
        {
            StartGameServerRPC();
        }
    }

    public PlayerController GetPlayerController(ulong clientId)
    {
        return PlayerControllers.FirstOrDefault(playerController => playerController.OwnerClientId == clientId);
    }

    public void StartPlayerController(PlayerController playerController)
    {
        PlayerControllers.Add(playerController);
        if (playerController.IsOwner)
        {
            ClientOwnerPlayerController = playerController;

            _gameState = GameState.GameSetup;

            OnClientPlayerControllerSetUp?.Invoke();
            OnClientPlayerControllerSetUp = null;
        }
    }


    [ServerRpc]
    [Command]
    public void StartGameServerRPC()
    {
        _gameState = GameState.GamePlay;
        StartGameClientRPC();

        LoadPlayerSetup();
        StartPlayerTurnClientRPC(PlayerControllers[_playerIdTurn.Value].OwnerClientId);
        
        PlayerControllers[_playerIdTurn.Value].PlayerTurnController.StartTurnServerRPC();
    }

    [ClientRpc]
    public void StartGameClientRPC()
    {
        OnGameStart?.Invoke();
    }

    private void LoadPlayerSetup()
    {
        foreach (var playerController in PlayerControllers)
        {
            foreach (var diceDescription in _incomeDiceDescriptions)
            {
                playerController.PlayerResourceController.AddIncomeServerRPC(diceDescription.GetDiceContainer());

                Debug.Log($"Dice {diceDescription.DiceID} : ");
            }

            foreach (var cardDescription in _deckCardDescriptions)
            {
                playerController.PlayerResourceController.AddCardToDeckServerRPC(cardDescription.GetCardContainer());

                Debug.Log($"Card {cardDescription.CardID} : ");
            }
        }
    }


    [ServerRpc]
    public void StartNextPlayerTurnServerRPC()
    {
        EndPlayerTurnClientRPC(PlayerControllers[_playerIdTurn.Value].OwnerClientId);

        _playerIdTurn.Value++;
        if (_playerIdTurn.Value >= PlayerControllers.Count)
        {
            _playerIdTurn.Value = 0;
        }

        PlayerControllers[_playerIdTurn.Value].PlayerTurnController.StartTurnServerRPC();

        StartPlayerTurnClientRPC(PlayerControllers[_playerIdTurn.Value].OwnerClientId);
    }

    [ClientRpc]
    private void EndPlayerTurnClientRPC(ulong clientId)
    {
        OnPlayerTurnEnd?.Invoke(GetPlayerController(clientId));
        Debug.Log($"Player {PlayerControllers[_playerIdTurn.Value].OwnerClientId} end turn");
    }

    [ClientRpc]
    private void StartPlayerTurnClientRPC(ulong clientId)
    {
        OnPlayerTurnStart?.Invoke(GetPlayerController(clientId));
        Debug.Log($"Player {PlayerControllers[_playerIdTurn.Value].OwnerClientId} start turn");
    }

    public void CheckWin(ulong ownerClientId, int victoryPoint)
    {
        if (victoryPoint >= _victoryPointRequirement)
        {
            EndGameServerRPC(ownerClientId);
        }
    }

    [ServerRpc(RequireOwnership = false),Command]
    public void EndGameServerRPC(ulong ownerClientId)
    {
        _gameState = GameState.GameEnd;
        EndGameClientRPC(ownerClientId);
    }

    [ClientRpc]
    private void EndGameClientRPC(ulong ownerClientId)
    {
        OnGameEnd?.Invoke();

        Debug.Log($"Game End {ownerClientId} Win!");
    }
}