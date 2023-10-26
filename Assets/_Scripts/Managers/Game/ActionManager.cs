
using System;
using System.Collections.Generic;
using _Scripts.Managers.Game;
using _Scripts.NetworkContainter;
using _Scripts.Player.Dice;
using _Scripts.Player.Pawn;
using _Scripts.Simulation;
using Shun_Unity_Editor;
using UnityEngine;
using UnityUtilities;

public class ActionManager : SingletonMonoBehaviour<ActionManager>
{
    
    [SerializeField, ShowImmutable] private List<ITargetee> _mapTargets = new();
    private List<ITargetee> _highlightingTargetees = new();
    
    
    public void AddTargetEntity(ITargetee targetEntity)
    {
        _mapTargets.Add(targetEntity);
    }
    
    public void RemoveTargetEntity(ITargetee targetEntity)
    {
        _mapTargets.Remove(targetEntity);
    }
    
    public void StartHighlightTargetee(ITargeter targeter, Func<ITargetee, bool> targeteeCondition)
    {
        foreach (var targetee in _mapTargets)
        {
            if (targeteeCondition.Invoke(targetee))
            {
                if(_highlightingTargetees.Contains(targetee)) continue;
                
                targetee.StartHighlight();
                _highlightingTargetees.Add(targetee);
            }
        }
    }

    public void EndHighlightTargetee()
    {
        foreach (var highlightingTargetee in _highlightingTargetees)
        {
            highlightingTargetee?.EndHighlight();
        }
        _highlightingTargetees.Clear();
    }
    
    
    
    public void ExecuteTarget<TTargeter, TTargetee>(TTargeter targeter, TTargetee targetee) 
        where TTargeter : ITargeter
        where TTargetee : ITargetee
    {
        if (targeter == null || targetee == null)
        {
            Debug.Log("Targeter or targetee is null");
            return;
        }
        
        SendTargetExecutionToServer(targeter, targetee);
        
    }

    public async void SimulateAction(ActionContainer actionContainer)
    {
        var targeter = GetTargeter(actionContainer.TargeterContainer);
        var targetee = GetTargetee(actionContainer.TargeteeContainer);

        SimulationManager.Instance.AddCoroutineSimulationObject(targeter.ExecuteTargeter(targetee));
        SimulationManager.Instance.AddCoroutineSimulationObject(targetee.ExecuteTargetee(targeter));
        
    }

    

    private ITargeter GetTargeter(TargetContainer targeterContainer)
    {
        switch (targeterContainer.TargetType)
        {
            case TargetType.Empty:
                break;
            case TargetType.Pawn:
                break;
            case TargetType.Tile:
                break;
            case TargetType.Card:
                return GetHandCard(targeterContainer.TargetContainerIndex, targeterContainer.TargetClientOwnerId);
                break;
            case TargetType.Dice:
                return GetHandDice(targeterContainer.TargetContainerIndex, targeterContainer.TargetClientOwnerId);
                break;
            case TargetType.Deck:
            default:
                return null;
        }

        return null;
    }
    
    private ITargetee GetTargetee(TargetContainer targeteeContainer)
    {
        switch (targeteeContainer.TargetType)
        {
            case TargetType.Empty:
                return GetEmptyTarget();
                break;
            case TargetType.Deck:
                return GetDeck();
                break;
            case TargetType.Pawn:
                return GetMapPawn(targeteeContainer.TargetContainerIndex);
                break;
            case TargetType.Tile:
                break;
            case TargetType.Card:
                return GetHandCard(targeteeContainer.TargetContainerIndex, targeteeContainer.TargetClientOwnerId);
                break;
            case TargetType.Dice:
                return GetHandDice(targeteeContainer.TargetContainerIndex, targeteeContainer.TargetClientOwnerId);
                break;
            default:
                return null;
        }

        return null;
    }


    private void SendTargetExecutionToServer<TTargeter, TTargetee>(TTargeter targeterMonoBehaviour, TTargetee targeteeMonoBehaviour)
        where TTargeter : ITargeter
        where TTargetee : ITargetee
    {
        var targetContainer = CreateActionContainer(targeterMonoBehaviour, targeteeMonoBehaviour);

        GameManager.Instance.ClientOwnerPlayerController.PlayerActionController.PlayTargetServerRPC(targetContainer);
    }


    private ActionContainer CreateActionContainer<TTargeter, TTargetee>(TTargeter targeter, TTargetee targetee)
        where TTargeter : ITargeter
        where TTargetee : ITargetee
    {
        
        TargetContainer targeterContainer = GetTargetContainer(targeter);
        TargetContainer targeteeContainer = GetTargetContainer(targetee);
        
        return new ActionContainer
        {
            TargeterContainer = targeterContainer,
            TargeteeContainer = targeteeContainer
        };
        
    }

    private TargetContainer GetTargetContainer(ITargeter targeter)
    {
        var monoBehavior = targeter.GetMonoBehavior();

        TargetType targetType = monoBehavior switch
        {
            HandDice => TargetType.Dice,
            MapPawn => TargetType.Pawn,
            PlayerDeck => TargetType.Deck,
            HandCard => TargetType.Card,
            _ => TargetType.Empty
        };

        return new TargetContainer
        {
            TargetType = targetType,
            TargetClientOwnerId = targeter.OwnerClientID,
            TargetContainerIndex = targeter.ContainerIndex
        };
        
    }
    
    
    private TargetContainer GetTargetContainer(ITargetee targeter)
    {
        var monoBehavior = targeter.GetMonoBehavior();
        
        TargetType targetType;
        if (monoBehavior is HandDice) targetType = TargetType.Dice;
        else if (monoBehavior is MapPawn) targetType = TargetType.Pawn;
        else if (monoBehavior is PlayerDeck) targetType = TargetType.Deck;
        else targetType = TargetType.Empty; 

        return new TargetContainer
        {
            TargetType = targetType,
            TargetClientOwnerId = targeter.OwnerClientID,
            TargetContainerIndex = targeter.ContainerIndex
        };
        
    }

    public HandDice GetHandDice(int diceContainerIndex, ulong ownerClientID)
    {
        return HandManager.Instance.GetPlayerDiceHand(ownerClientID)
            .GetHandDice(diceContainerIndex);
    }
    
    public HandCard GetHandCard(int cardContainerIndex, ulong ownerClientID)
    {
        return HandManager.Instance.GetPlayerCardHand(ownerClientID)
            .GetHandCard(cardContainerIndex);
    }
    
    public MapPawn GetMapPawn(int pawnContainerIndex)
    {
        return MapManager.Instance.GetPlayerPawn(pawnContainerIndex);
    }

    public List<ITargetee> GetMapPawns(Func<ITargetee, bool> selectCondition = null)
    {
        List<ITargetee> mapPawns = new List<ITargetee>();
        foreach (var mapTarget in _mapTargets)
        {
            if (selectCondition == null || selectCondition.Invoke(mapTarget))
            {
                mapPawns.Add(mapTarget);
            }
        }

        return mapPawns;
    }
    
    
    public PlayerDeck GetDeck()
    {
        return MapManager.Instance.GetDeck();
    }
    
    private ITargetee GetEmptyTarget()
    {
        return MapManager.Instance.GetEmptyTarget();
    }

}
