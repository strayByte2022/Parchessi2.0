using System;
using System.Collections.Generic;
using _Scripts.Managers.Game;
using _Scripts.NetworkContainter;
using _Scripts.Player.Dice;
using _Scripts.Player.Pawn;
using _Scripts.Scriptable_Objects;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Scripts.Player
{
    public class PlayerDiceHand : PlayerControllerCompositionDependency
    {
        [SerializeField] private int _maxDices = 3;
        [SerializeField] private float _diceSpawnRadius = 1f;

        private readonly Dictionary<int, HandDice>
            _containerIndexToHandDiceDictionary = new Dictionary<int, HandDice>();

        private HandDiceRegion _handDiceRegion;
        
        public override void Initialize(PlayerController playerController)
        {
            base.Initialize(playerController);
            _handDiceRegion = GetComponent<HandDiceRegion>();
        }
        public HandDice GetHandDice(int diceContainerIndex)
        {
            _containerIndexToHandDiceDictionary.TryGetValue(diceContainerIndex, out var handDice);
            return handDice;
        }


        public void AddDiceToHand(DiceContainer diceContainer, int diceContainerIndex)
        {
            if (_containerIndexToHandDiceDictionary.Count >= _maxDices) return;
            var handDice = CreateDiceHand(diceContainer, diceContainerIndex);
            _containerIndexToHandDiceDictionary.Add(diceContainerIndex, handDice);
            
        }


        public HandDice CreateDiceHand(DiceContainer diceContainer, int diceContainerIndex)
        {
            var diceDescription = GameResourceManager.Instance.GetDiceDescription(diceContainer.DiceID);
            var randomX = Random.Range(-1f, 1f);
            var randomY = Random.Range(-1f, 1f);
            var randomPosition = new Vector3(randomX, randomY, 0f) * _diceSpawnRadius;
            var handDice = Instantiate(diceDescription.GetHandDicePrefab(), transform.position + randomPosition, Quaternion.identity, transform);
            handDice.Initialize(this, diceDescription, diceContainerIndex, PlayerController.OwnerClientId);
            
            return handDice;
        }

        public void PlayDice(HandDice handDice)
        {
            if (IsOwner)
            {
                PlayerController.PlayerResourceController.RemoveDiceServerRPC(handDice.ContainerIndex);
            }
            else
            {
                RemoveDiceFromRegion(handDice);
            }

            _containerIndexToHandDiceDictionary.Remove(handDice.ContainerIndex);
        }



        public void ConvertToCard(HandDice handDice)
        {
            if (IsOwner)
            {
                PlayerController.PlayerResourceController.RemoveDiceServerRPC(handDice.ContainerIndex);
                PlayerController.PlayerResourceController.AddCardToHandServerRPC();
            }
            else
            {
                RemoveDiceFromRegion(handDice);
            }

            _containerIndexToHandDiceDictionary.Remove(handDice.ContainerIndex);
        }

        public void RollDice(HandDice handDice, int lowerRange, int upperRange)
        {
            if (IsOwner)
            {
                PlayerController.PlayerActionController.RollDiceServerRPC(handDice.ContainerIndex, lowerRange, upperRange);
            }
        }

        public void RemoveDiceFromRegion(HandDice handDice)
        {
            var handDiceDragAndTargeter = handDice.GetComponent<HandDiceDragAndTargeter>();
            _handDiceRegion.RemoveCard(handDiceDragAndTargeter);
            
            handDice.transform.SetParent(transform);
        }
        
        public void AddDiceToRegion(HandDice handDice)
        {
            var handDiceDragAndTargeter = handDice.GetComponent<HandDiceDragAndTargeter>();
            _handDiceRegion.TryAddCard(handDiceDragAndTargeter);
            
        }
    }


}