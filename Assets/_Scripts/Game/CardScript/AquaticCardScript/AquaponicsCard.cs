using System.Collections;
using System.Collections.Generic;
using _Scripts.DataWrapper;
using _Scripts.Managers.Game;
using _Scripts.Player.Card;
using _Scripts.Player.Dice;
using _Scripts.Player.Pawn;
using _Scripts.Scriptable_Objects;
using _Scripts.Simulation;
using DG.Tweening;
using UnityEngine;

public class AquaponicsCard : StylizedHandCard
{
    public ObservableData<int> HealValue;
    
    protected override void InitializeCardDescription(CardDescription cardDescription)
    {
        base.InitializeCardDescription(cardDescription);
        HealValue = new ObservableData<int>(cardDescription.CardEffectIntVariables[0]);
    }
    
    public override bool CheckTargeteeValid(ITargetee targetee)
    {
        if (targetee is MapPawn mapPawn)
        {
            return targetee.OwnerClientID == this.OwnerClientID;
        }
        else return false;
    }
    
    public override SimulationPackage ExecuteTargeter<TTargetee>(TTargetee targetee)
    {
        var package = new SimulationPackage();

        if (targetee is not MapPawn playerPawn)
        {
            return package; 
        }
        
        package.AddToPackage(HandCardFace.SetCardFace(CardFaceType.Front));
        package.AddToPackage(MoveToMiddleScreen());

        package.AddToPackage(() =>
            {
                // Inherit this class and write Card effect
                Debug.Log(name + " Card drag to Pawn " + playerPawn.name);

                if (AudioPlayer.instance != null)
                {
                    AudioPlayer.instance.PlaySound(AudioPlayer.instance.click);
                }

                MapManager.Instance.HealPawnServerRPC(OwnerClientID, HealValue.Value, playerPawn.ContainerIndex);
                
                PlayerCardHand.PlayCard(this);

                Destroy();
                
            });
        
        
        return package;
    }
}
