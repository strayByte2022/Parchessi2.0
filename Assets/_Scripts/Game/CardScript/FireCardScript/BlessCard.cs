using _Scripts.DataWrapper;
using _Scripts.Managers.Game;
using _Scripts.NetworkContainter;
using _Scripts.Player.Card;
using _Scripts.Player.Pawn;
using _Scripts.Scriptable_Objects;
using _Scripts.Simulation;

namespace _Scripts.CardScript.AquaticCardScript
{
    public class BlessCard : StylizedHandCard
    {
        public ObservableData<int> SpeedBonus;
        public ObservableData<int> BuffDuration;

        protected override void InitializeCardDescription(CardDescription cardDescription)
        {
            base.InitializeCardDescription(cardDescription);
            SpeedBonus = new ObservableData<int>(cardDescription.CardEffectIntVariables[0]);
            BuffDuration = new ObservableData<int>(cardDescription.CardEffectIntVariables[1]);
        }

        public override bool CheckTargeteeValid(ITargetee targetee)
        {
            if (targetee.TargetType == TargetType.Empty)
            {
                return true;
            }

            return false;
        }

        public bool GetAllPawn(ITargetee targetee)
        {
            if (targetee is MapPawn mapPawn)
            {
                return true;
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

            var selectedPawns = ActionManager.Instance.GetMapPawns(GetAllPawn);

            foreach (var pawn in selectedPawns)
            {
                package.AddToPackage(() =>
                {
                    var pawnStatEffectContainer = new PawnStatEffectContainer()
                    {
                        EffectDuration = BuffDuration.Value,
                        EffectType = PawnStatEffectType.Speed,
                        EffectValue = SpeedBonus.Value,
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