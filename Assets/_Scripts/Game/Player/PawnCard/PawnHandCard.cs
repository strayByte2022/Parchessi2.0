using System.Collections;
using System.Collections.Generic;
using _Scripts.DataWrapper;
using _Scripts.Managers.Game;
using _Scripts.Player.Card;
using _Scripts.Player.Dice;
using _Scripts.Player.Pawn;
using _Scripts.Scriptable_Objects;
using _Scripts.Simulation;
using UnityEngine;

public class PawnHandCard : HandCard
{
    public PawnDescription PawnDescription { get; protected set; }

    public ObservableData<int> Attack = new ObservableData<int>();
    public ObservableData<int> MaxHealth = new ObservableData<int>();
    public ObservableData<int> Speed = new ObservableData<int>();
    

    protected override void InitializeCardDescription(CardDescription cardDescription)
    {
        base.InitializeCardDescription(cardDescription);
        var pawnCardDescription = cardDescription as PawnCardDescription;

        if (pawnCardDescription != null)
            InitializedPawnCardDescription(pawnCardDescription);
        else
            Debug.LogError("PawnCardDescription is null");
    }

    private void InitializedPawnCardDescription(PawnCardDescription pawnCardDescription)
    {
        PawnDescription = pawnCardDescription.PawnDescription;
        
        Attack.Value = PawnDescription.PawnAttackDamage;
        MaxHealth.Value = PawnDescription.PawnMaxHealth;
        Speed.Value = PawnDescription.PawnMovementSpeed;
    }
    
    public override SimulationPackage ExecuteTargeter<TTargetee>(TTargetee targetee)
    {
        var package = new SimulationPackage();
        
        package.AddToPackage(HandCardFace.SetCardFace(HandCardFace.CardFaceType.Front));
        package.AddToPackage(() =>
        {
            if (targetee is PlayerEmptyTarget playerEmptyTarget)
            {
                // Inherit this class and write Card effect
                Debug.Log(name + " Card drag to Empty ");
                PlayerCardHand.PlayCard(this);

                MapManager.Instance.SpawnPawnToMap(PawnDescription, OwnerClientID);

                Destroy();
            }
            else if (targetee is MapPawn playerPawn)
            {
                // Inherit this class and write Card effect
                Debug.Log(name + " Card drag to Pawn " + playerPawn.name);
                PlayerCardHand.PlayCard(this);

                Destroy();
            }
        });


        return package;
    }
}