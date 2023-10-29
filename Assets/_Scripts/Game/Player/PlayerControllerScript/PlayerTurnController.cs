using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.NetworkContainter;
using _Scripts.Player;
using QFSW.QC;
using Shun_Unity_Editor;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerTurnController : PlayerControllerRequireDependency
{
    public enum PlayerPhase : byte
    {
        WaitPhase,
        PreparationPhase,
        RollPhase,
        SubsequencePhase
    }

    private PlayerController _playerController;

    public readonly NetworkVariable<PlayerPhase> CurrentPlayerPhase = new (PlayerPhase.WaitPhase);
    public readonly NetworkVariable<int> CurrentPlayerTurn = new (0);
    public readonly NetworkVariable<int> VictoryPoint = new (0);

    private bool _isFirstRollPhase = true;
    
    private PlayerDiceHand _playerDiceHand;
    private void Start()
    {
        _playerDiceHand = FindObjectOfType<PlayerDiceHand>();

        CurrentPlayerPhase.OnValueChanged += (PlayerPhase oldValue, PlayerPhase newValue) => GameManager.Instance.OnPlayerPhaseChanged.Invoke(oldValue, newValue, PlayerController);
    }

    [Command]
    public PlayerPhase GetPlayerPhase()
    {
        return CurrentPlayerPhase.Value;
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void StartTurnServerRPC()
    {
        _isFirstRollPhase = true;
        StartPreparationPhaseServerRPC();
        
    }

    [ServerRpc]
    public void StartPreparationPhaseServerRPC()
    {
        CurrentPlayerPhase.Value = PlayerPhase.PreparationPhase;
        StartPreparationPhaseClientRPC();
    }

    
    [ClientRpc]
    private void StartPreparationPhaseClientRPC()
    {
        Debug.Log($"Client {OwnerClientId} Start PreparationPhase");
    }

    
    [ServerRpc]
    public void StartRollPhaseServerRPC()
    {
        CurrentPlayerPhase.Value = PlayerPhase.RollPhase;
        
        if (_isFirstRollPhase) PlayerResourceController.GainIncomeServerRPC();
        PlayerResourceController.GainBonusDiceServerRPC();
        
        StartRollPhaseClientRPC();
    }
   
    [ClientRpc]
    private void StartRollPhaseClientRPC()
    {
        Debug.Log($"Client {OwnerClientId} Start RollPhase");
        
    }

    [ServerRpc]
    public void EndRollPhaseServerRPC()
    {
        if (PlayerController.PlayerResourceController.CheckEmptyPlayingDices())
        {
            StartSubsequencePhaseServerRPC();
        }
        else
        {
            FailEndRollPhaseClientRPC();
        }
    }

    [ClientRpc]
    public void FailEndRollPhaseClientRPC()
    {
        // Not used all dices yet
        Debug.Log("Not used all dices yet");
    }

    [ServerRpc]
    private void StartSubsequencePhaseServerRPC()
    {
        CurrentPlayerPhase.Value = PlayerPhase.SubsequencePhase;
        StartSubsequencePhaseClientRPC();
    }
    
    [ClientRpc]
    private void StartSubsequencePhaseClientRPC()
    {
        Debug.Log($"Client {OwnerClientId} Start Subsequence Phase");
    }
    
    [ServerRpc]
    public void EndSubsequencePhaseServerRPC()
    {
        if (PlayerController.PlayerResourceController.CheckEmptyBonusDices())
        { 
            EndTurnServerRPC();
        }
        else
        { 
            StartPreparationPhaseServerRPC();
        }
    }
    
    
    [ClientRpc]
    private void EndTurnClientRPC()
    {
        Debug.Log($"Client {OwnerClientId} End Turn");
    }
    
    [ServerRpc(RequireOwnership = false), Command]
    private void EndTurnServerRPC()
    {
        CurrentPlayerPhase.Value = PlayerPhase.WaitPhase;
        EndTurnClientRPC();
        
        GameManager.Instance.StartNextPlayerTurnServerRPC();
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddVictoryPointServerRPC(int value)
    {
        VictoryPoint.Value += value;
        GameManager.Instance.CheckWin(OwnerClientId, VictoryPoint.Value);
    }
}
