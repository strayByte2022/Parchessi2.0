using _Scripts.DataWrapper;
using _Scripts.Managers.Game;
using _Scripts.NetworkContainter;
using _Scripts.Player.Card;
using _Scripts.Player.Pawn;
using _Scripts.Scriptable_Objects;
using _Scripts.Simulation;

namespace _Scripts.CardScript.AquaticCardScript
{
    public class GraceCard : StylizedHandCard
    {
        public ObservableData<int> SpeedBuff;
        public ObservableData<int> BuffDuration;


        protected override void InitializeCardDescription(CardDescription cardDescription)
        {
            base.InitializeCardDescription(cardDescription);
            SpeedBuff = new ObservableData<int>(cardDescription.CardEffectIntVariables[0]);
            BuffDuration = new ObservableData<int>(cardDescription.CardEffectIntVariables[1]);
        }

        public override bool CheckTargeteeValid(ITargetee targetee)
        {
            if (targetee.TargetType == TargetType.Empty)
            {
                return true;
            }

            return GetAllAlly(targetee);
        }

        public bool GetAllAlly(ITargetee targetee)
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

            var selectedPawns = ActionManager.Instance.GetMapPawns(GetAllAlly);

            foreach (var pawn in selectedPawns)
            {
                package.AddToPackage(() =>
                {
                    // Inherit this class and write Card effect
                    var pawnStatEffectContainer = new PawnStatEffectContainer()
                    {
                        EffectDuration = BuffDuration.Value,
                        EffectType = PawnStatEffectType.Speed,
                        EffectValue = SpeedBuff.Value,
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