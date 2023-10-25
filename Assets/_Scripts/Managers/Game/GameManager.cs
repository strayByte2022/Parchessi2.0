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
    public Action OnClientPlayerJoinGameSetUp { get; set; }
    public Action OnGameStart { get; set; }
    public Action OnGameEnd { get; set; }
    public Action OnGamePause { get; set; }
    public Action OnGameResume { get; set; }
    public Action OnGameQuit { get; set; }
    
    public Action<PlayerController> OnPlayerTurnStart { get; set; }
    public Action<PlayerTurnController.PlayerPhase, PlayerTurnController.PlayerPhase, PlayerController> OnPlayerPhaseChanged { get; set;}
    public Action<PlayerController> OnPlayerTurnEnd { get; set; }

    [SerializeField] private List<DiceDescription> _incomeDiceDescriptions = new();
    [SerializeField] private List<CardDescription> _deckCardDescriptions = new();
    [SerializeField] private int _victoryPointRequirement = 4;

    public void Start()
    {
        //NetworkManager.Singleton.OnClientConnectedCallback += CreatePlayerControllerServerRPC;
        CreatePlayerControllerServerRPC(NetworkManager.LocalClientId);

        if (IsServer)
        {
            StartCoroutine(WaitForAllClientConnect());
        }
    }

    private IEnumerator WaitForAllClientConnect()
    {
        yield return new WaitUntil(() => PlayerControllers.Count == GameMultiplayerManager.Instance.GetAllPlayerContainer().Length);
        StartGameServerRPC();
    }

    [ServerRpc(RequireOwnership = false)]
    private void CreatePlayerControllerServerRPC(ulong clientId)
    {
        var playerController = Instantiate(GameResourceManager.Instance.PlayerControllerPrefab);
        playerController.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        CreatePlayerControllerClientRPC(clientId);
    }

    [ClientRpc]
    private void CreatePlayerControllerClientRPC(ulong clientId)
    {
        var networkObject = NetworkManager.SpawnManager.GetPlayerNetworkObject(clientId);
        var playerController = networkObject.GetComponent<PlayerController>();
        playerController.Initialize();
        StartPlayerController(playerController);
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
            
            OnClientPlayerJoinGameSetUp?.Invoke();
            OnClientPlayerJoinGameSetUp = null;
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
        StartPlayerTurn(PlayerControllers[_playerIdTurn.Value]);
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

        StartPlayerTurn(PlayerControllers[_playerIdTurn.Value]);
        
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
    
    private void StartPlayerTurn(PlayerController playerController)
    {
        playerController.PlayerTurnController.StartPreparationPhaseServerRPC();
        playerController.PlayerResourceController.GainIncomeServerRPC();
    }

    public void CheckWin(ulong ownerClientId , int victoryPoint)
    {
        if (victoryPoint >= _victoryPointRequirement)
        {
            EndGameServerRPC(ownerClientId);
        }
        
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void EndGameServerRPC(ulong ownerClientId)
    {
        _gameState = GameState.GameEnd;
        EndGameClientRPC(ownerClientId);
    }
    
    [ClientRpc]
    private void EndGameClientRPC(ulong ownerClientId)
    {
        OnGameEnd?.Invoke();
        
        Debug.Log("Game End!");
    }
    
}