using _Scripts.DataWrapper;
using _Scripts.Managers.Game;
using _Scripts.Player.Pawn;
using _Scripts.Scriptable_Objects;
using _Scripts.Simulation;
using UnityEngine;

namespace _Scripts.CardScript.AquaticCardScript
{
    public class UpstreamSwimCard : StylizedHandCard
    {
        public ObservableData<int> SpeedBonusValue;
        public ObservableData<int> BuffTurnCount;

        protected override void InitializeCardDescription(CardDescription cardDescription)
        {
            base.InitializeCardDescription(cardDescription);
            SpeedBonusValue = new ObservableData<int>(cardDescription.CardEffectIntVariables[0]);
            BuffTurnCount = new ObservableData<int>(cardDescription.CardEffectIntVariables[1]);
        }

        public override bool CheckTargeteeValid(ITargetee targetee)
        {
            if (targetee is MapPawn mapPawn)
            {
                return targetee.OwnerClientID != this.OwnerClientID;
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

                MapManager.Instance.TakeDamagePawnServerRPC(SpeedBonusValue.Value, playerPawn.ContainerIndex);
                
                PlayerCardHand.PlayCard(this);

                Destroy();
                
            });
        
        
            return package;
        }
    }
}