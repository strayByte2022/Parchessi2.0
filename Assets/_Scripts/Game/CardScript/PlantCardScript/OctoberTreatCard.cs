using _Scripts.DataWrapper;
using _Scripts.Managers.Game;
using _Scripts.NetworkContainter;
using _Scripts.Player.Pawn;
using _Scripts.Scriptable_Objects;
using _Scripts.Simulation;
using UnityEngine;

namespace _Scripts.CardScript.AquaticCardScript
{
    public class OctoberTreatCard : StylizedHandCard
    {
        public ObservableData<int> MaxHPUp;
        public ObservableData<int> BuffTurnCount;

        protected override void InitializeCardDescription(CardDescription cardDescription)
        {
            base.InitializeCardDescription(cardDescription);
            MaxHPUp = new ObservableData<int>(cardDescription.CardEffectIntVariables[0]);
            BuffTurnCount = new ObservableData<int>(cardDescription.CardEffectIntVariables[1]);
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
                        EffectType = PawnStatEffectType.Health,
                        EffectValue = MaxHPUp.Value,
                        EffectedOwnerClientID = pawn.OwnerClientID,
                        EffectedPawnContainerIndex = pawn.ContainerIndex,
                        TriggerOwnerClientID = OwnerClientID
                    };

                    MapManager.Instance.AddStatEffectServerRPC(pawnStatEffectContainer);
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
}