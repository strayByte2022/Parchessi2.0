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

public class BloodRushCard : StylizedHandCard
{
    public ObservableData<int> SpeedBonus;
    public ObservableData<int> BuffTurnCount;
    public ObservableData<int> Demerit;

    protected override void InitializeCardDescription(CardDescription cardDescription)
    {
        base.InitializeCardDescription(cardDescription);
        SpeedBonus = new ObservableData<int>(cardDescription.CardEffectIntVariables[0]);
        BuffTurnCount = new ObservableData<int>(cardDescription.CardEffectIntVariables[1]);
        Demerit = new ObservableData<int>(CardDescription.CardEffectIntVariables[2]);
    }

    public override bool CheckTargeteeValid(ITargetee targetee)
    {
        if (targetee.TargetType == TargetType.Empty)
        {
            return true;
        }

        return GetAllAllyPawn(targetee);
    }

    private bool GetAllAllyPawn(ITargetee targetee)
    {
        if (targetee is MapPawn mapPawn)
        {
            return mapPawn.OwnerClientID == this.OwnerClientID;
        }

        return false;
    }

    public override SimulationPackage ExecuteTargeter<TTargetee>(TTargetee targetee)
    {
        var package = new SimulationPackage();

        if (targetee.TargetType != TargetType.Empty)
        {
            return package;
        }

        package.AddToPackage(HandCardFace.SetCardFace(CardFaceType.Front));
        package.AddToPackage(MoveToMiddleScreen());

        var selectedPawns = ActionManager.Instance.GetMapPawns(GetAllAllyPawn);

        foreach (var pawn in selectedPawns)
        {
            package.AddToPackage(() =>
            {
                // Inherit this class and write Card effect

                var pawnStatEffectContainer = new PawnStatEffectContainer()
                {
                    EffectDuration = BuffTurnCount.Value,
                    EffectType = PawnStatEffectType.Speed,
                    EffectValue = SpeedBonus.Value,
                    EffectedOwnerClientID = pawn.OwnerClientID,
                    EffectedPawnContainerIndex = pawn.ContainerIndex,
                    TriggerOwnerClientID = OwnerClientID
                };

                MapManager.Instance.AddStatEffectServerRPC(pawnStatEffectContainer);
                MapManager.Instance.TakeDamagePawnServerRPC(OwnerClientID, Demerit.Value, pawn.ContainerIndex);
            });
        }

        package.AddToPackage(() =>
        {
            PlayerCardHand.PlayCard(this);

            Destroy();
        });


        return package;
    }
}
