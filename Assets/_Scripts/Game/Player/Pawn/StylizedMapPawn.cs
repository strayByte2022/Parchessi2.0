using _Scripts.NetworkContainter;
using _Scripts.Simulation;
using DG.Tweening;
using System;
using UnityEngine;

namespace _Scripts.Player.Pawn
{
    public class StylizedMapPawn : MapPawn
    {
        [SerializeField] private float _moveDuration = 0.25f;
        [SerializeField] private Ease _moveEase = Ease.Linear;
        private SkeletonAnimationController _skeletonAnimationController;

        private void Awake()
        {
            _skeletonAnimationController = GetComponent<SkeletonAnimationController>();
        }

        private Tween MoveTween(Transform position )
        {
            return transform.DOMove(position.position, _moveDuration).SetEase(_moveEase);
        }
        
        public override SimulationPackage StartMove(int startMapCellIndex, int stepCount)
        {
            var simulationPackage = new SimulationPackage();

            if (TryMove(stepCount))
            {
                simulationPackage.AddToPackage(() =>
                {
                    // Start move
                    _skeletonAnimationController.DoMoveAnim();
                    MapPath.Path[startMapCellIndex].RemovePawn(this);
                });
                for (int step = 1; step < stepCount; step++)
                {
                    // Teleport to the end position
                    StandingMapCellIndex = step + startMapCellIndex;
                    
                    simulationPackage.AddToPackage(MoveTween(MapPath.Path[StandingMapCellIndex].GetEmptySpot()));
                }
                
                simulationPackage.AddToPackage(() =>
                {
                    StandingMapCellIndex ++;
                    // Make combat to all pawn in the cell
                    foreach (var mapPawn in MapPath.Path[StandingMapCellIndex].GetAllPawn())
                    {
                        if (this.OwnerClientID == mapPawn.OwnerClientID) continue;
                        MapManager.MakeCombatServerRPC(ContainerIndex, mapPawn.ContainerIndex);
                    }
                    
                });

                
                simulationPackage.AddToPackage(() =>
                {
                    // End move
                    MapManager.EndMovePawnServerRPC(ContainerIndex, StandingMapCellIndex);
                });
            }
            else
            {
                
            }

            return simulationPackage;
         
        }
        
        
        public override SimulationPackage EndMove(int endMapCellIndex)
        {
            var simulationPackage = new SimulationPackage();
            
            
            StandingMapCellIndex = endMapCellIndex;
            simulationPackage.AddToPackage(MoveTween(MapPath.Path[StandingMapCellIndex].GetEmptySpot()));
            
            simulationPackage.AddToPackage(() =>
            {
                
                if (endMapCellIndex == MapPath.Path.Count - 1) // If the pawn reach the end of the path
                {
                    _skeletonAnimationController.DoVictoryAnim();
                    MapManager.ReachGoalServerRPC(ContainerIndex, OwnerClientID);
                }
                else
                {
                    // End move
                    _skeletonAnimationController.DoIdleAnim();
                    MapPath.Path[endMapCellIndex].EnterPawn(this);
                }
            });
            
            return simulationPackage;

        }
        public override SimulationPackage TakeDamage(int damage)
        {
            
            var simulationPacket = new SimulationPackage();

            simulationPacket.AddToPackage(() =>
            {
                CurrentHealth.Value -= damage;
                _skeletonAnimationController.DoHurtAnim();
                if (CurrentHealth.Value <= 0)
                {
                    // Death

                    MapPath.Path[StandingMapCellIndex].RemovePawn(this);

                    MapManager.RemovePawnFromMapServerRPC(ContainerIndex);

                }
            });

            return simulationPacket;
        }
        public override SimulationPackage AddStatEffect(PawnStatEffectContainer pawnStatEffectContainer)
        {
            if (pawnStatEffectContainer.EffectedPawnContainerIndex != ContainerIndex) return null;

            Action addStatEffect = GetAddStatEffectAction(pawnStatEffectContainer);

            if (pawnStatEffectContainer.EffectValue > 0)
            {
                _skeletonAnimationController.DoBuffAnim();
                return AddBuff(addStatEffect);
            }
            else
            {
                _skeletonAnimationController.DoDebuffAnim();
                return AddDebuff(addStatEffect);
            }
        }
    }
}