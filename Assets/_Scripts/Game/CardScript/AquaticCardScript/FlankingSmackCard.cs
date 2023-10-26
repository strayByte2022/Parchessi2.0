using _Scripts.DataWrapper;
using _Scripts.Managers.Game;
using _Scripts.Player.Card;
using _Scripts.Player.Pawn;
using _Scripts.Scriptable_Objects;
using _Scripts.Simulation;

namespace _Scripts.CardScript.AquaticCardScript
{
    public class FlankingSmackCard : StylizedHandCard
    {
        public ObservableData<int> DamageValue;
    
        protected override void InitializeCardDescription(CardDescription cardDescription)
        {
            base.InitializeCardDescription(cardDescription);
            DamageValue = new ObservableData<int>(cardDescription.CardEffectIntVariables[0]);
        }
    
        public override bool CheckTargeteeValid(ITargetee targetee)
        {
            if (targetee.TargetType == TargetType.Empty)
            {
                return true;
            }
            else return false;
        }

        public bool GetAllPawnNotFullHealth(ITargetee targetee)
        {
            if (targetee is MapPawn mapPawn)
            {
                return mapPawn.CurrentHealth.Value < mapPawn.MaxHealth.Value;
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

            package.AddToPackage(HandCardFace.SetCardFace(HandCardFace.CardFaceType.Front));
            package.AddToPackage(MoveToMiddleScreen());

            var selectedPawns = ActionManager.Instance.GetMapPawns(GetAllPawnNotFullHealth);
            
            foreach (var pawn in selectedPawns)
            {
                package.AddToPackage(() =>
                {
                    // Inherit this class and write Card effect
                    MapManager.Instance.TakeDamagePawnServerRPC(DamageValue.Value, pawn.ContainerIndex);
                    
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