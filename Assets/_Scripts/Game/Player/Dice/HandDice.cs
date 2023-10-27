using System;
using System.Collections;
using _Scripts.DataWrapper;
using _Scripts.Managers.Game;
using _Scripts.Player;
using _Scripts.Player.Dice;
using _Scripts.Player.Pawn;
using _Scripts.Simulation;
using Shun_Card_System;
using Shun_Unity_Editor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HandDice : PlayerEntity, ITargeter
{

    public Action OnTargeterDestroy { get; set; }
    public ObservableData<int> DiceValue;

    private PlayerDiceHand _playerDiceHand;
    private HandDiceRoll _handDiceRoll;
    [SerializeField, ShowImmutable] DiceDescription _diceDescription;
    private ITargeter _targeterImplementation;

    public void Initialize(PlayerDiceHand playerDiceHand, DiceDescription diceDescription, int containerIndex, ulong ownerClientID )
    {
        _playerDiceHand = playerDiceHand;
        _handDiceRoll = GetComponent<HandDiceRoll>();
        _diceDescription = diceDescription;
        base.Initialize(containerIndex, ownerClientID);
        
    }

    private void Start()
    {
        _playerDiceHand.RollDice(this, _diceDescription.DiceLowerRange, _diceDescription.DiceUpperRange);
    }

    
    public bool CheckTargeteeValid(ITargetee targetee)
    {
        if (targetee is PlayerDeck) return true;
        if (targetee is MapPawn mapPawn)
        {
            return mapPawn.TryMove(DiceValue.Value);
        }
        else return false;
    }
    
    public virtual SimulationPackage RollDice(int endValue)
    {
        var simulationPackage = new SimulationPackage();
        simulationPackage.AddToPackage(() => { 
            _playerDiceHand.RemoveDiceFromRegion(this);
            _handDiceRoll.RollDice(endValue, (int endValue) =>
            {
                _playerDiceHand.AddDiceToRegion(this);
                DiceValue.Value = endValue;
            });});
        
        return simulationPackage;
    }
    
    
    public virtual SimulationPackage SetDiceValue(int value)
    {
        var simulationPackage = new SimulationPackage();
        simulationPackage.AddToPackage(() =>
        {
            DiceValue.Value = value;
            Debug.Log($"Dice Value: {DiceValue.Value}");
        });
        return simulationPackage;
    }
    

    public virtual SimulationPackage ExecuteTargeter<TTargetee>(TTargetee targetee) where TTargetee : ITargetee
    {
        var package = new SimulationPackage();
        package.AddToPackage(() =>
        {
            if (targetee is MapPawn mapPawn)
            {
                _playerDiceHand.PlayDice(this);
                
                MapManager.Instance.StartMovePawnServerRPC(mapPawn.ContainerIndex, DiceValue.Value);
                // Inherit this class and write Dice effect
                Debug.Log(name + " Dice drag to Pawn " + mapPawn.name);

            }
            else if (targetee is PlayerDeck)
            {
                Debug.Log("Draw a card");

                _playerDiceHand.ConvertToCard(this);
                
            }

            Destroy();
        });
        
        return package;
    }


    public void Destroy()
    {
        
        if (TryGetComponent<BaseDraggableObject>(out var baseDraggableObject))
            baseDraggableObject.Destroy();
        Destroy(gameObject);
    }
    
}
