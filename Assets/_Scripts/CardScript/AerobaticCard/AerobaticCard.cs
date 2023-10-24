using System.Collections;
using System.Collections.Generic;
using _Scripts.DataWrapper;
using _Scripts.Player.Dice;
using _Scripts.Player.Pawn;
using _Scripts.Scriptable_Objects;
using _Scripts.Simulation;
using UnityEngine;

public class AerobaticCard : HandCard
{
    public ObservableData<int> DealDamage;

    protected override void InitializeCardDescription(CardDescription cardDescription)
    {
        base.InitializeCardDescription(cardDescription);
        DealDamage = new ObservableData<int>(cardDescription.CardEffectIntVariables[0]);
    }
    
    public override SimulationPackage ExecuteTargeter<TTargetee>(TTargetee targetee)
    {
        var package = new SimulationPackage();
        
        if (targetee is MapPawn playerPawn)
        {
            package.AddToPackage(() =>
            {
                // Inherit this class and write Card effect
                Debug.Log(name + " Card drag to Pawn " + playerPawn.name);

                playerPawn.TakeDamage(DealDamage.Value);
                
                PlayerCardHand.PlayCard(this);

                Destroy();
                
            });
        }
        else if (targetee is PlayerEmptyTarget playerEmptyTarget)
        {
            package.AddToPackage(() =>
            {
            
                // Inherit this class and write Card effect
                Debug.Log(name + " Card drag to Empty ");
                PlayerCardHand.PlayCard(this);

                Destroy();
                
            });
        }
        
        
        return package;
    }
}
