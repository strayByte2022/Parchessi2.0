using _Scripts.DataWrapper;
using _Scripts.Managers.Game;
using _Scripts.NetworkContainter;
using _Scripts.Player.Pawn;
using _Scripts.Scriptable_Objects;
using _Scripts.Simulation;
using UnityEngine;

namespace _Scripts.CardScript.AquaticCardScript
{
    public class LastWordCard : StylizedHandCard
    {
        public ObservableData<int> DealDamage;

        protected override void InitializeCardDescription(CardDescription cardDescription)
        {
            base.InitializeCardDescription(cardDescription);
            
        }

        public override bool CheckTargeteeValid(ITargetee targetee)
        {
            if (targetee.TargetType == TargetType.Empty)
            {
                return true;
            }

            return GetAllEnemyPawnNotFullHealth(targetee);
        }

        private bool GetAllEnemyPawnNotFullHealth(ITargetee targetee)
        {
            if (targetee is MapPawn mapPawn)
            {
                return mapPawn.OwnerClientID != this.OwnerClientID && mapPawn.CurrentHealth.Value < mapPawn.MaxHealth.Value;
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

            var selectedPawns = ActionManager.Instance.GetMapPawns(GetAllEnemyPawnNotFullHealth);

            foreach (var pawn in selectedPawns)
            {
                package.AddToPackage(() =>
                {
                    // Inherit this class and write Card effect

                    MapManager.Instance.TakeDamagePawnServerRPC(OwnerClientID, DealDamage.Value, pawn.ContainerIndex);
                });
            }

            package.AddToPackage(() =>
            {
                PlayerCardHand.PlayCard(this);
                if (AudioPlayer.instance != null)
                {
                    AudioPlayer.instance.PlaySound(AudioPlayer.instance.rock);
                }
                Destroy();
            });



            return package;
        }
    }
}