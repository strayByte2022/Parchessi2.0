using System.Collections;
using System.Collections.Generic;
using _Scripts.DataWrapper;
using _Scripts.Managers.Game;
using _Scripts.NetworkContainter;
using _Scripts.Player.Card;
using _Scripts.Player.Dice;
using _Scripts.Player.Pawn;
using _Scripts.Scriptable_Objects;
using _Scripts.Simulation;
using DG.Tweening;
using UnityEngine;

public class RefreshCard : StylizedHandCard
{
    public ObservableData<int> BuffTurnCount;

    protected override void InitializeCardDescription(CardDescription cardDescription)
    {
        base.InitializeCardDescription(cardDescription);
        BuffTurnCount = new ObservableData<int>(cardDescription.CardEffectIntVariables[0]);
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
            int SpeedBuffValue = playerPawn.MaxHealth.Value - playerPawn.PawnDescription.PawnMaxHealth;

            var pawnStatEffectContainer = new PawnStatEffectContainer()
            {
                EffectDuration = 1,
                EffectType = PawnStatEffectType.Speed,
                EffectValue = SpeedBuffValue,
                EffectedOwnerClientID = playerPawn.OwnerClientID,
                EffectedPawnContainerIndex = playerPawn.ContainerIndex,
                TriggerOwnerClientID = OwnerClientID
            };

            MapManager.Instance.AddStatEffectServerRPC(pawnStatEffectContainer);

            PlayerCardHand.PlayCard(this);
            if (AudioPlayer.instance != null)
            {
                AudioPlayer.instance.PlaySound(AudioPlayer.instance.leaf);
            }
            Destroy();

        });


        return package;
    }
}
