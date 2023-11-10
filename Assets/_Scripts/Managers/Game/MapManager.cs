using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Managers.Network;
using _Scripts.Map;
using _Scripts.NetworkContainter;
using _Scripts.Player.Dice;
using _Scripts.Player.Pawn;
using _Scripts.Simulation;
using QFSW.QC;
using Unity.Netcode;
using UnityEngine;
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;

namespace _Scripts.Managers.Game
{
    public class MapManager : SingletonNetworkBehavior<MapManager>
    {
        private const int MAP_PAWN_COUNT = 20;
        private static readonly PawnContainer EmptyPawnContainer = new PawnContainer{PawnID = -1};
        
        [SerializeField] private MapRegion _mapRegion;
        
        [SerializeField] private PlayerDeck _playerDeck;
        [SerializeField] private PlayerEmptyTarget _playerEmptyTarget;
        
        private NetworkList<PawnContainer> _mapPawnContainers;
        private NetworkList<PawnStatEffectContainer> _pawnStatEffectContainers;
        
        private readonly Dictionary<int, MapPawn> _containerIndexToMapPawnDictionary = new();
        
        
        private void Awake()
        {
            _mapPawnContainers = new NetworkList<PawnContainer>(Enumerable.Repeat(EmptyPawnContainer, MAP_PAWN_COUNT).ToArray());
            _pawnStatEffectContainers = new NetworkList<PawnStatEffectContainer>();
            
            GameManager.Instance.OnPlayerTurnStart += UpdateStatEffect;
        }

        private void UpdateStatEffect(PlayerController obj)
        {
            if (obj == GameManager.Instance.ClientOwnerPlayerController)
            {
                UpdateStatEffectServerRPC(NetworkManager.LocalClientId);
            }
        }

        public MapPawn GetPlayerPawn(int pawnContainerIndex)
        {
            _containerIndexToMapPawnDictionary.TryGetValue(pawnContainerIndex, out var mapPawn);
            return mapPawn;
        }

        public PlayerDeck GetDeck()
        {
            return _playerDeck;
        }
        
        public ITargetee GetEmptyTarget()
        {
            return _playerEmptyTarget;
        }

        public void SpawnPawnToMap(PawnDescription pawnDescription, ulong ownerClientId)
        {
            var pawnContainer = pawnDescription.GetPawnContainer();
            pawnContainer.ClientOwnerID = ownerClientId;
            pawnContainer.StandingMapCell = 0;
            
            SpawnPawnToMapServerRPC(pawnContainer, ownerClientId);
        }
        
        private MapPawn CreateMapPawn(PawnContainer pawnContainer, int pawnContainerIndex, ulong ownerClientId)
        {
            var pawnDescription = GameResourceManager.Instance.GetPawnDescription(pawnContainer.PawnID);
            var mapPath = _mapRegion.GetMapPath((int)ownerClientId);
            Transform spawnTransform = mapPath.Path[0].transform;
            var mapPawn = Instantiate(pawnDescription.GetMapPawnPrefab(), spawnTransform.position, spawnTransform.rotation, _mapRegion.transform);
            
            mapPawn.Initialize(mapPath, pawnDescription,  pawnContainerIndex, ownerClientId);
            
            return mapPawn;
        }


        [ServerRpc(RequireOwnership = false)]
        private void SpawnPawnToMapServerRPC(PawnContainer pawnContainer, ulong ownerClientId, ServerRpcParams serverRpcParams = default)
        {
            var clientId = serverRpcParams.Receive.SenderClientId;
            if (!NetworkManager.ConnectedClients.ContainsKey(clientId)) return;

            //if (NetworkManager.ServerClientId != clientId) return;
            if (ownerClientId != clientId) return;
            
            foreach (var mapPawnContainer in _mapPawnContainers)
            {
                if (mapPawnContainer.Equals(pawnContainer))
                {
                    return; // Already Spawned
                }
            }
            
            // Spawn Logic
            for (var index = 0; index < _mapPawnContainers.Count; index++)
            {
                var mapPawnContainer = _mapPawnContainers[index];
                if (mapPawnContainer.Equals(EmptyPawnContainer))
                {
                    _mapPawnContainers[index] = pawnContainer;
                    SpawnPawnToMapClientRPC(pawnContainer, index, ownerClientId);
                    break;
                }
            }
        }

        [ClientRpc]
        private void SpawnPawnToMapClientRPC(PawnContainer pawnContainer, int containerIndex, ulong ownerClientId)
        {
            var mapPawn = CreateMapPawn(pawnContainer, containerIndex, ownerClientId);
            _containerIndexToMapPawnDictionary.Add(containerIndex, mapPawn);
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void RemovePawnFromMapServerRPC(int pawnContainerIndex, ServerRpcParams serverRpcParams = default)
        {
            var clientId = serverRpcParams.Receive.SenderClientId;
            if (!NetworkManager.ConnectedClients.ContainsKey(clientId)) return;
            
            var mapPawnContainer = _mapPawnContainers[pawnContainerIndex];
            if (mapPawnContainer.ClientOwnerID != clientId) return;
            //if (NetworkManager.ServerClientId != clientId) return;
            
            _mapPawnContainers[pawnContainerIndex] = EmptyPawnContainer;
            
            RemovePawnFromMapClientRPC(pawnContainerIndex);
        }
        
        [ClientRpc]
        public void RemovePawnFromMapClientRPC(int pawnContainerIndex)
        {
            var mapPawn = GetPlayerPawn(pawnContainerIndex);
            _containerIndexToMapPawnDictionary.Remove(pawnContainerIndex);
            SimulationManager.Instance.AddSimulationPackage(mapPawn.Die());
        }

        [ServerRpc(RequireOwnership = false)]
        public void StartMovePawnServerRPC(int pawnContainerIndex, int stepCount, ServerRpcParams serverRpcParams = default)
        {
            var clientId = serverRpcParams.Receive.SenderClientId;
            if (!NetworkManager.ConnectedClients.ContainsKey(clientId)) return;
            
            var mapPawnContainer = _mapPawnContainers[pawnContainerIndex];
            if (mapPawnContainer.ClientOwnerID != clientId) return;
            //if (NetworkManager.ServerClientId != clientId) return;
            
            // Start Move Logic

            stepCount += mapPawnContainer.PawnStatContainer.MovementSpeed;
            
            StartMovePawnClientRPC(pawnContainerIndex, mapPawnContainer.StandingMapCell, stepCount);
            
        }
        
        
        [ClientRpc]
        private void StartMovePawnClientRPC(int pawnContainerIndex, int startMapCellIndex ,int stepCount)
        {
            var mapPawn = GetPlayerPawn(pawnContainerIndex);
            
            SimulationManager.Instance.AddSimulationPackage(mapPawn.StartMove(startMapCellIndex, stepCount));
        }
        
        

        [ServerRpc(RequireOwnership = false)]
        public void EndMovePawnServerRPC(int pawnContainerIndex, int finalMapCellIndex, ServerRpcParams serverRpcParams = default)
        {
            var clientId = serverRpcParams.Receive.SenderClientId;
            if (!NetworkManager.ConnectedClients.ContainsKey(clientId)) return;
            
            var mapPawnContainer = _mapPawnContainers[pawnContainerIndex];
            if (mapPawnContainer.ClientOwnerID != clientId) return;
            //if (NetworkManager.ServerClientId != clientId) return;
            
            // End Move Logic
            
            mapPawnContainer.StandingMapCell = finalMapCellIndex;
            _mapPawnContainers[pawnContainerIndex] = mapPawnContainer;
            
            EndMovePawnClientRPC(pawnContainerIndex, finalMapCellIndex);
        }

        [ClientRpc]
        private void EndMovePawnClientRPC(int pawnContainerIndex, int finalMapCellIndex)
        {
            var mapPawn = GetPlayerPawn(pawnContainerIndex);
            
            SimulationManager.Instance.AddSimulationPackage(mapPawn.EndMove(finalMapCellIndex));
            
        }


        [ServerRpc(RequireOwnership = false)]
        public void MakeCombatServerRPC(int attackerPawnContainerIndex, int defenderPawnContainerIndex, ServerRpcParams serverRpcParams = default)
        {
            var clientId = serverRpcParams.Receive.SenderClientId;
            if (!NetworkManager.ConnectedClients.ContainsKey(clientId)) return;
            //if (NetworkManager.ServerClientId != clientId) return;
            
            var attackerPawnContainer = _mapPawnContainers[attackerPawnContainerIndex];
            if (attackerPawnContainer.ClientOwnerID != clientId) return;   
            
            // Attack Logic
            var defenderPawnContainer = _mapPawnContainers[defenderPawnContainerIndex];
            
            defenderPawnContainer.PawnStatContainer.CurrentHealth -= Mathf.Max(attackerPawnContainer.PawnStatContainer.AttackDamage,0);
            _mapPawnContainers[defenderPawnContainerIndex] = defenderPawnContainer;
            
            MakeCombatClientRPC(attackerPawnContainerIndex, defenderPawnContainerIndex);
        }
        
        [ClientRpc]
        private void MakeCombatClientRPC(int attackerPawnContainerIndex, int defenderPawnContainerIndex)
        {
            var attackerMapPawn = GetPlayerPawn(attackerPawnContainerIndex);
            var defenderMapPawn = GetPlayerPawn(defenderPawnContainerIndex);
            
            SimulationManager.Instance.AddSimulationPackage(attackerMapPawn.Attack(defenderMapPawn));
        }

        [ServerRpc(RequireOwnership = false)]
        public void TakeDamagePawnServerRPC(ulong dealerOwnerClientId, int damage, int defenderPawnContainerIndex, ServerRpcParams serverRpcParams = default)
        {
            var clientId = serverRpcParams.Receive.SenderClientId;
            if (!NetworkManager.ConnectedClients.ContainsKey(clientId)) return;
            //if (NetworkManager.ServerClientId != clientId) return;
            
            if (dealerOwnerClientId != clientId) return;   
            
            // Attack Logic
            var defenderPawnContainer = _mapPawnContainers[defenderPawnContainerIndex];
            
            defenderPawnContainer.PawnStatContainer.CurrentHealth -= Mathf.Max(damage, 0);
            _mapPawnContainers[defenderPawnContainerIndex] = defenderPawnContainer;
            
            TakeDamagePawnClientRPC(damage, defenderPawnContainerIndex);
        }
        
        [ClientRpc]
        private void TakeDamagePawnClientRPC(int damage, int defenderPawnContainerIndex)
        {
            var defenderMapPawn = GetPlayerPawn(defenderPawnContainerIndex);
            
            SimulationManager.Instance.AddSimulationPackage(defenderMapPawn.TakeDamage(damage));
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void HealPawnServerRPC(ulong healerOwnerClientId, int healValue, int healedPawnContainerIndex, ServerRpcParams serverRpcParams = default)
        {
            var clientId = serverRpcParams.Receive.SenderClientId;
            if (!NetworkManager.ConnectedClients.ContainsKey(clientId)) return;
            
            if (healerOwnerClientId != clientId) return;   
            
            // Attack Logic
            var defenderPawnContainer = _mapPawnContainers[healedPawnContainerIndex];

            var maxHealableAmount = defenderPawnContainer.PawnStatContainer.MaxHealth -
                                    defenderPawnContainer.PawnStatContainer.CurrentHealth;
            var actualHealValue = Mathf.Min(Mathf.Max(healValue, 0) , (maxHealableAmount));
            defenderPawnContainer.PawnStatContainer.CurrentHealth += actualHealValue;
            _mapPawnContainers[healedPawnContainerIndex] = defenderPawnContainer;
            
            HealPawnClientRPC(healValue, healedPawnContainerIndex);
        }
        
        [ClientRpc]
        private void HealPawnClientRPC(int healValue, int defenderPawnContainerIndex)
        {
            var defenderMapPawn = GetPlayerPawn(defenderPawnContainerIndex);
            
            SimulationManager.Instance.AddSimulationPackage(defenderMapPawn.Heal(healValue));
        }


        [ServerRpc(RequireOwnership = false)]
        public void ReachGoalServerRPC(int containerIndex, ulong ownerClientId, ServerRpcParams serverRpcParams = default)
        {
            var clientId = serverRpcParams.Receive.SenderClientId;
            if (!NetworkManager.ConnectedClients.ContainsKey(clientId)) return;
            
            if (ownerClientId != clientId) return;   
            //if (NetworkManager.ServerClientId != clientId) return;
            
            // Win Logic
            _mapPawnContainers[containerIndex] = EmptyPawnContainer;
            PlayerTurnController playerTurnController = GameManager.Instance.GetPlayerController(ownerClientId).PlayerTurnController;
            playerTurnController.AddVictoryPointServerRPC(1);
            
            ReachGoalClientRPC(containerIndex, ownerClientId);
            
        }
        
        [ClientRpc]
        private void ReachGoalClientRPC(int containerIndex, ulong ownerClientId)
        {
            var mapPawn = GetPlayerPawn(containerIndex);
            _containerIndexToMapPawnDictionary.Remove(containerIndex);
            SimulationManager.Instance.AddSimulationPackage(mapPawn.ReachGoal());
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void UpdateStatEffectServerRPC(ulong ownerClientId, ServerRpcParams serverRpcParams = default)
        {
            var clientId = serverRpcParams.Receive.SenderClientId;
            if (!NetworkManager.ConnectedClients.ContainsKey(clientId)) return;
            
            if (ownerClientId != clientId) return;
            
            for (var index = _pawnStatEffectContainers.Count - 1; index >= 0; index--)
            {
                var pawnStatEffectContainer = _pawnStatEffectContainers[index];
                if (pawnStatEffectContainer.EffectedOwnerClientID != ownerClientId) continue;

                if (pawnStatEffectContainer.EffectDuration > 1)
                {
                    pawnStatEffectContainer.EffectDuration--;
                    _pawnStatEffectContainers[index] = pawnStatEffectContainer;
                }
                else
                {
                    _pawnStatEffectContainers.RemoveAt(index);
                    RemoveStatEffect(pawnStatEffectContainer);
                    TimeOutStatEffectClientRPC(pawnStatEffectContainer);
                }
            }
        }
        
        [ClientRpc]
        private void TimeOutStatEffectClientRPC(PawnStatEffectContainer effectContainer)
        {
            var mapPawn = GetPlayerPawn(effectContainer.EffectedPawnContainerIndex);
            SimulationManager.Instance.AddSimulationPackage(mapPawn.TimeOutStatEffect(effectContainer));
        }
        

        [ServerRpc(RequireOwnership = false)]
        public void AddStatEffectServerRPC(PawnStatEffectContainer effectContainer , ServerRpcParams serverRpcParams = default)
        {
            var clientId = serverRpcParams.Receive.SenderClientId;
            if (!NetworkManager.ConnectedClients.ContainsKey(clientId)) return;
            
            if (effectContainer.EffectedOwnerClientID != clientId) return;
            
            _pawnStatEffectContainers.Add(effectContainer);

            AddStatEffect(effectContainer);
            AddStatEffectClientRPC(effectContainer);
        }
        
        [ClientRpc]
        private void AddStatEffectClientRPC(PawnStatEffectContainer effectContainer)
        {
            var mapPawn = GetPlayerPawn(effectContainer.EffectedPawnContainerIndex);
            SimulationManager.Instance.AddSimulationPackage( mapPawn.AddStatEffect(effectContainer));
        }

        private void AddStatEffect(PawnStatEffectContainer effectContainer)
        {
            var pawnContainer = _mapPawnContainers[effectContainer.EffectedPawnContainerIndex];
            switch (effectContainer.EffectType)
            {
                case PawnStatEffectType.Attack:
                    pawnContainer.PawnStatContainer.AttackDamage += effectContainer.EffectValue;
                    break;
                case PawnStatEffectType.Health:
                    pawnContainer.PawnStatContainer.MaxHealth += effectContainer.EffectValue;
                    pawnContainer.PawnStatContainer.CurrentHealth += effectContainer.EffectValue;
                    break;
                case PawnStatEffectType.Speed:
                    pawnContainer.PawnStatContainer.MovementSpeed += effectContainer.EffectValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            _mapPawnContainers[effectContainer.EffectedPawnContainerIndex] = pawnContainer;
        }
        
        private void RemoveStatEffect(PawnStatEffectContainer effectContainer)
        {
            var pawnContainer = _mapPawnContainers[effectContainer.EffectedPawnContainerIndex];
            switch (effectContainer.EffectType)
            {
                case PawnStatEffectType.Attack:
                    pawnContainer.PawnStatContainer.AttackDamage -= effectContainer.EffectValue;
                    break;
                case PawnStatEffectType.Health:
                    pawnContainer.PawnStatContainer.MaxHealth -= effectContainer.EffectValue;
                    pawnContainer.PawnStatContainer.CurrentHealth = Mathf.Min(pawnContainer.PawnStatContainer.CurrentHealth, pawnContainer.PawnStatContainer.MaxHealth);
                    break;
                case PawnStatEffectType.Speed:
                    pawnContainer.PawnStatContainer.MovementSpeed -= effectContainer.EffectValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            _mapPawnContainers[effectContainer.EffectedPawnContainerIndex] = pawnContainer;
        }
    }
}