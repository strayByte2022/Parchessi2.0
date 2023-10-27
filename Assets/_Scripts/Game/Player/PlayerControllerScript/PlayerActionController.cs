using _Scripts.NetworkContainter;
using _Scripts.Simulation;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerActionController : PlayerControllerRequireDependency
{
    NetworkList <ActionContainer> _targetContainers;
    
    public override void Awake()
    {
        base.Awake();
        _targetContainers = new();
    }

    
    [ServerRpc]
    public void RollDiceServerRPC(int containerIndex, int lowerBound, int upperBound)
    {
        var dice = PlayerResourceController.PlayingDices[containerIndex];
        dice.Value = Random.Range(lowerBound, upperBound);
        PlayerResourceController.PlayingDices[containerIndex] = dice;
        RollDiceClientRPC(containerIndex, dice.Value);
    }

    [ClientRpc]
    private void RollDiceClientRPC(int containerIndex, int value)
    {
        HandDice handDice = ActionManager.Instance.GetHandDice(containerIndex, OwnerClientId);
        SimulationManager.Instance.AddSimulationPackage( handDice.RollDice(value));
    }

    
    
    [ServerRpc]
    public void PlayTargetServerRPC(ActionContainer actionContainer)
    {
        _targetContainers.Add(actionContainer);
        PlayTargetClientRPC(actionContainer);
    }
    
    [ClientRpc]
    private void PlayTargetClientRPC(ActionContainer actionContainer)
    {
        ActionManager.Instance.SimulateAction(actionContainer);
    }
    
    
    
}
