using System;
using System.Collections.Generic;
using _Scripts.DataWrapper;
using _Scripts.Managers.Game;
using _Scripts.Map;
using _Scripts.Simulation;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace _Scripts.Player.Pawn
{
    public class MapPawn : PlayerEntity
    {
        protected MapManager MapManager => MapManager.Instance;
        protected MapPath MapPath;
        private PawnDescription _pawnDescription;

        public int StandingMapCellIndex = 0;

        public ObservableData<int> AttackDamage;
        public ObservableData<int> MaxHealth;
        public ObservableData<int> CurrentHealth;
        public ObservableData<int> MovementSpeed;

        public virtual void Initialize(MapPath playerMapPawn, PawnDescription pawnDescription , int containerIndex, ulong ownerClientId)
        {
            MapPath = playerMapPawn;
            _pawnDescription = pawnDescription;

            Initialize(containerIndex, ownerClientId);
            LoadPawnDescription();
        }

        public virtual void LoadPawnDescription()
        {
            AttackDamage = new ObservableData<int>(_pawnDescription.PawnAttackDamage);
            MaxHealth = new ObservableData<int>(_pawnDescription.PawnMaxHealth);
            CurrentHealth = new ObservableData<int>(_pawnDescription.PawnMaxHealth);
            MovementSpeed = new ObservableData<int>(_pawnDescription.PawnMovementSpeed);
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
                // Buff from attacker and debuff from defender
                
                SimulationManager.Instance.AddSimulationPackage(defenderMapPawn.TakeDamage(AttackDamage.Value));
            });
            
            
            
            return null;
        }

        public virtual SimulationPackage TakeDamage(MapPawn attackerMapPawn)
        {
            var simulationPacket = new SimulationPackage();
            
            simulationPacket.AddToPackage(() =>
            {
                // Buff from attacker and debuff from defender
                
                SimulationManager.Instance.AddSimulationPackage(TakeDamage(attackerMapPawn.AttackDamage.Value));
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
    }
}