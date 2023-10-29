using System;
using System.Collections.Generic;
using _Scripts.DataWrapper;
using _Scripts.Managers.Game;
using _Scripts.Map;
using _Scripts.NetworkContainter;
using _Scripts.Simulation;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Action = System.Action;

namespace _Scripts.Player.Pawn
{
    public class MapPawn : PlayerEntity
    {
        protected MapManager MapManager => MapManager.Instance;
        protected MapPath MapPath;
        public PawnDescription PawnDescription { get; protected set; }

        public int StandingMapCellIndex = 0;

        public ObservableData<int> AttackDamage = new ();
        public ObservableData<int> MaxHealth = new();
        public ObservableData<int> CurrentHealth = new();
        public ObservableData<int> MovementSpeed = new();

        public virtual void Initialize(MapPath playerMapPawn, PawnDescription pawnDescription , int containerIndex, ulong ownerClientId)
        {
            MapPath = playerMapPawn;
            PawnDescription = pawnDescription;

            Initialize(containerIndex, ownerClientId);
            LoadPawnDescription();
        }

        protected virtual void LoadPawnDescription()
        {
            AttackDamage.Value = (PawnDescription.PawnAttackDamage);
            MaxHealth.Value = PawnDescription.PawnMaxHealth;
            CurrentHealth.Value = PawnDescription.PawnMaxHealth;
            MovementSpeed.Value = PawnDescription.PawnMovementSpeed;
        }

        

        public virtual bool TryMove(int stepCount)
        {
            int startMapCellIndex = StandingMapCellIndex;
            int endMapCellIndex = stepCount + startMapCellIndex;

            if (startMapCellIndex + stepCount >= MapPath.Path.Count) return false;
                
            for (var index = startMapCellIndex + 1; index < stepCount + startMapCellIndex; index++)
            {
                var mapCell = MapPath.Path[index];

                if (index < MapPath.Path.Count && mapCell.CheckEnterable()) continue;

                return false;

            }
            
            return TryMakeCombatToFindEmptySlot(this, MapPath.Path[endMapCellIndex]);
        }

        protected virtual bool TryMakeCombatToFindEmptySlot(MapPawn attacker, MapCell mapCell)
        {
            bool emptySlot = mapCell.CheckEnterable();
            var defenders = mapCell.GetAllPawn();
            foreach (var defender in defenders)
            {
                if (defender.OwnerClientID == attacker.OwnerClientID) continue;
                
                int damage = attacker.AttackDamage.Value;
                int currentHealth = defender.CurrentHealth.Value;
                
                if (currentHealth - damage <= 0)
                {
                    emptySlot = true; 
                }
            }

            return emptySlot; // If there is an empty slot, the attacker can move to that slot
        }
        
        public virtual SimulationPackage StartMove(int startMapCellIndex, int stepCount)
        {
            var simulationPackage = new SimulationPackage();

            if (TryMove(stepCount))
            {
                
                simulationPackage.AddToPackage(()=> 
                {
                    // Start move
                    MapPath.Path[startMapCellIndex].RemovePawn(this);
                    
                    for (int step = 1; step < stepCount; step++)
                    {
                    
                        // Teleport to the end position
                        StandingMapCellIndex = step + startMapCellIndex;
                        transform.position = MapPath.Path[StandingMapCellIndex].GetEmptySpot().transform.position; 
                    }

                    StandingMapCellIndex ++;
                });

  
                simulationPackage.AddToPackage(() =>
                {
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
        
        

        public virtual SimulationPackage EndMove(int endMapCellIndex)
        {
            
            var simulationPackage = new SimulationPackage();
            
            simulationPackage.AddToPackage(() =>
            {
                // Teleport to the end position
                StandingMapCellIndex = endMapCellIndex;
                transform.position = MapPath.Path[endMapCellIndex].transform.position;

                if (endMapCellIndex == MapPath.Path.Count - 1) // If the pawn reach the end of the path
                {
                    MapManager.Instance.ReachGoalServerRPC(ContainerIndex, OwnerClientID);
                }
                else
                {
                    // End move
                    MapPath.Path[endMapCellIndex].EnterPawn(this);
                }
            });
            
            return simulationPackage;

        }
        

        public virtual SimulationPackage ExecuteTargetee<TTargeter>(TTargeter targeter) where TTargeter : ITargeter
        {
            return null;
        }


        public virtual SimulationPackage Attack(MapPawn defenderMapPawn)
        {
            var simulationPacket = new SimulationPackage();
            
            simulationPacket.AddToPackage(() =>
            {
                // AddBuff from attacker and debuff from defender
                
                SimulationManager.Instance.AddSimulationPackage(defenderMapPawn.TakeDamage(AttackDamage.Value));
            });
            
            return null;
        }
        
        
        public virtual SimulationPackage TakeDamage(int damage)
        {
            var simulationPacket = new SimulationPackage();
            
            simulationPacket.AddToPackage(() =>
            {
                CurrentHealth.Value -= damage;
                
                if (CurrentHealth.Value <= 0)
                {
                    // Death
                    
                    MapPath.Path[StandingMapCellIndex].RemovePawn(this);
                    
                    MapManager.RemovePawnFromMapServerRPC(ContainerIndex);
                    
                }
            });
            
            return simulationPacket;
        }
        
        
        public virtual SimulationPackage Death()
        {
            var simulationPacket = new SimulationPackage();
            
            simulationPacket.AddToPackage(() =>
            {
                // Death Animation
                Destroy(gameObject);
            });
            
            return simulationPacket;
        }

        public virtual SimulationPackage ReachGoal()
        {
            var simulationPacket = new SimulationPackage();
            
            simulationPacket.AddToPackage(() =>
            {
                // Fun Animation
                Debug.Log("Reach Goal!");
                MapPath.Path[StandingMapCellIndex].RemovePawn(this);
            });
            
            return simulationPacket;
        }

        public virtual SimulationPackage Heal(int healValue)
        {
            var simulationPacket = new SimulationPackage();
            
            simulationPacket.AddToPackage(() =>
            {
                // Fun Animation
                Debug.Log("Heal!");
                CurrentHealth.Value += healValue;
            });
            
            return simulationPacket;
        }

        public virtual SimulationPackage AddStatEffect(PawnStatEffectContainer pawnStatEffectContainer)
        {
            if (pawnStatEffectContainer.EffectedPawnContainerIndex != ContainerIndex) return null;

            Action addStatEffect = GetAddStatEffectAction(pawnStatEffectContainer);
            
            if (pawnStatEffectContainer.EffectValue > 0)
            {
                return AddBuff(addStatEffect);
            }
            else
            {
                return AddDebuff(addStatEffect);
            }
        }

        protected virtual SimulationPackage AddBuff(Action buffAction)
        {
            var simulationPacket = new SimulationPackage();
            
            simulationPacket.AddToPackage(() =>
            {
                // Fun Animation
                Debug.Log("AddBuff!");
                buffAction.Invoke();
            });
            
            return simulationPacket;
        }

        protected virtual SimulationPackage AddDebuff(Action debuffAction)
        {
            var simulationPacket = new SimulationPackage();
            
            simulationPacket.AddToPackage(() =>
            {
                // Fun Animation
                Debug.Log("AddDebuff!");
                debuffAction.Invoke();
                
            });
            
            return simulationPacket;
        }
        
        public virtual SimulationPackage TimeOutStatEffect(PawnStatEffectContainer pawnStatEffectContainer)
        {
            if (pawnStatEffectContainer.EffectedPawnContainerIndex != ContainerIndex) return null;

            Action removeStatEffect = GetRemoveStatEffectAction(pawnStatEffectContainer);
            
            if (pawnStatEffectContainer.EffectValue > 0)
            {
                return RemoveBuff(removeStatEffect);
            }
            else
            {
                return RemoveDebuff(removeStatEffect);
            }
        }
        
        protected virtual SimulationPackage RemoveBuff(Action buffAction)
        {
            var simulationPacket = new SimulationPackage();
            
            simulationPacket.AddToPackage(() =>
            {
                // Fun Animation
                Debug.Log("RemoveBuff!");
                buffAction.Invoke();
            });
            
            return simulationPacket;
        }
        
        protected virtual SimulationPackage RemoveDebuff(Action debuffAction)
        {
            var simulationPacket = new SimulationPackage();
            
            simulationPacket.AddToPackage(() =>
            {
                // Fun Animation
                Debug.Log("RemoveDebuff!");
                debuffAction.Invoke();
                
            });
            
            return simulationPacket;
        }



        protected virtual Action GetAddStatEffectAction(PawnStatEffectContainer pawnStatEffectContainer)
        {
            Action addStatEffect = null;
            switch (pawnStatEffectContainer.EffectType)
            {
                case PawnStatEffectType.Attack:
                    addStatEffect += () =>
                    {
                        AttackDamage.Value += pawnStatEffectContainer.EffectValue;
                    };
                    break;
                case PawnStatEffectType.Health:
                    addStatEffect += () =>
                    {
                        MaxHealth.Value += pawnStatEffectContainer.EffectValue;
                        CurrentHealth.Value += pawnStatEffectContainer.EffectValue;
                    };
                    break;
                case PawnStatEffectType.Speed:
                    addStatEffect += () =>
                    {
                        MovementSpeed.Value += pawnStatEffectContainer.EffectValue;
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return addStatEffect;
        }
        
        protected virtual Action GetRemoveStatEffectAction(PawnStatEffectContainer pawnStatEffectContainer)
        {
            Action removeStatEffect = null;
            switch (pawnStatEffectContainer.EffectType)
            {
                case PawnStatEffectType.Attack:
                    removeStatEffect += () =>
                    {
                        AttackDamage.Value -= pawnStatEffectContainer.EffectValue;
                    };
                    break;
                case PawnStatEffectType.Health:
                    removeStatEffect += () =>
                    {
                        MaxHealth.Value -= pawnStatEffectContainer.EffectValue;
                        CurrentHealth.Value = Mathf.Min(MaxHealth.Value, CurrentHealth.Value);
                    };
                    break;
                case PawnStatEffectType.Speed:
                    removeStatEffect += () =>
                    {
                        MovementSpeed.Value -= pawnStatEffectContainer.EffectValue;
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return removeStatEffect;
        }
        
    }
}