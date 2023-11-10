﻿using System;
using System.Collections.Generic;
using _Scripts.Player;
using _Scripts.Player.Dice;
using DG.Tweening;
using Mono.CSharp;
using Shun_Unity_Editor;
using UnityEngine;
using UnityUtilities;

namespace _Scripts.Managers.Game
{
    public class HandManager : SingletonMonoBehaviour<HandManager>
    {
        
        [Header("Transform Parents")]
        [SerializeField] private Transform _playerCardHandParent;
        [SerializeField] private Transform _playerDiceHandParent;
        [SerializeField] private Transform _playerPeakCardHandParent;
        [SerializeField] private Transform _offScreenCardHandParent;
        [SerializeField] private Transform _offScreenDiceHandParent;
        
        [Header("Layer Mask Mouse Input")]
        [SerializeField] private LayerMask _cardLayerMask;
        [SerializeField] private LayerMask _diceLayerMask;
        
        private HandDraggableObjectMouseInput _diceDragMouseInput;
        private HandDraggableObjectMouseInput _cardDragMouseInput;
        private bool _isDiceHandInteractable = false;
        private bool _isCardHandInteractable = false;
        
        private Dictionary<ulong,PlayerCardHand> _playerCardHands = new ();
        private Dictionary<ulong,PlayerDiceHand> _playerDiceHands = new ();
        
        [Header("Tween")]
        [SerializeField] private float _moveDuration = 0.25f;
        [SerializeField] private Ease _moveEase = Ease.OutCubic;
        
        private Tweener _diceHandTween;
        private Tweener _cardHandTween;
        private PlayerDiceHand _tweenPlayerDiceHand;
        private PlayerCardHand _tweenPlayerCardHand;
        
        
        private void Awake()
        {
            _diceDragMouseInput = new HandDraggableObjectMouseInput(_diceLayerMask);
            _cardDragMouseInput = new HandDraggableObjectMouseInput(_cardLayerMask);
            
            GameManager.Instance.OnGameStart += OnGameStartSetUp;
            
            GameManager.Instance.OnPlayerPhaseChanged += ChangePhaseHand;
        }


        private void Update()
        {
            if (_isDiceHandInteractable)
                _diceDragMouseInput.UpdateMouseInput();
            
            if (_isCardHandInteractable)
                _cardDragMouseInput.UpdateMouseInput();
        }
        
        
        public PlayerDiceHand GetPlayerDiceHand(ulong clientOwnerID)
        {
            return _playerDiceHands[clientOwnerID];
        }
        
        public PlayerCardHand GetPlayerCardHand(ulong clientOwnerID)
        {
            return _playerCardHands[clientOwnerID];
        }
        
        private void OnGameStartSetUp()
        {
            foreach (var playerController in GameManager.Instance.PlayerControllers)
            {
                if (_playerCardHands.ContainsKey(playerController.OwnerClientId) || _playerDiceHands.ContainsKey(playerController.OwnerClientId)) continue;
                var playerCardHand = Instantiate(GameResourceManager.Instance.PlayerCardHandPrefab, _offScreenCardHandParent);
                var playerDiceHand = Instantiate(GameResourceManager.Instance.PlayerDiceHandPrefab, _offScreenDiceHandParent);
                
                playerDiceHand.Initialize(playerController);
                playerCardHand.Initialize(playerController);
                playerController.PlayerResourceController.InitializeHand(playerDiceHand, playerCardHand);
                
                _playerCardHands.Add(playerController.OwnerClientId, playerCardHand);
                _playerDiceHands.Add(playerController.OwnerClientId, playerDiceHand);
                
                HidePlayerHand(playerController);
            }
        }
        
        private void ChangePhaseHand(PlayerTurnController.PlayerPhase oldValue, PlayerTurnController.PlayerPhase newValue, PlayerController playerController)
        {
            _isCardHandInteractable = newValue == PlayerTurnController.PlayerPhase.RollPhase && playerController.IsOwner;
            _isDiceHandInteractable = newValue is PlayerTurnController.PlayerPhase.PreparationPhase or PlayerTurnController.PlayerPhase.SubsequencePhase
                                      && playerController.IsOwner;
            
            
            var playerCardHand = _playerCardHands[playerController.OwnerClientId];
            var playerDiceHand = _playerDiceHands[playerController.OwnerClientId];
            
            switch (newValue)
            {
                case PlayerTurnController.PlayerPhase.WaitPhase:
                    HidePlayerHand(playerController);
                    break;
                case PlayerTurnController.PlayerPhase.PreparationPhase:
                    ShowCardHand(playerCardHand);
                    HideDiceHand(playerDiceHand);
                    break;
                case PlayerTurnController.PlayerPhase.RollPhase:
                    PeakCardHand(playerCardHand);
                    ShowDiceHand(playerDiceHand);
                    break;
                case PlayerTurnController.PlayerPhase.SubsequencePhase:
                    ShowCardHand(playerCardHand);
                    HideDiceHand(playerDiceHand);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newValue), newValue, null);
            }
        }

        private void ShowPlayerHand(PlayerController playerController)
        {
            ShowCardHand(_playerCardHands[playerController.OwnerClientId]);
            ShowDiceHand(_playerDiceHands[playerController.OwnerClientId]);
        }
        
        private void HidePlayerHand(PlayerController playerController)
        {
            HideCardHand(_playerCardHands[playerController.OwnerClientId]);
            HideDiceHand(_playerDiceHands[playerController.OwnerClientId]);
        }
        
        private void PeakCardHand(PlayerCardHand playerCardHand)
        {
            // Kill the previous tween if it's still active
            
            if (_cardHandTween != null && _cardHandTween.IsActive() && _tweenPlayerCardHand == playerCardHand)
                _cardHandTween.Kill();
            
            playerCardHand.transform.SetParent(_playerPeakCardHandParent);
            _cardHandTween = playerCardHand.transform.DOMove(_playerPeakCardHandParent.position, _moveDuration)
                .SetEase(_moveEase);
            
            _tweenPlayerCardHand = playerCardHand;
        }

        private void ShowCardHand(PlayerCardHand playerCardHand)
        {
            // Kill the previous tween if it's still active
            
            if (_cardHandTween != null && _cardHandTween.IsActive() && _tweenPlayerCardHand == playerCardHand)
                _cardHandTween.Kill();

            playerCardHand.transform.SetParent(_playerCardHandParent);
            _cardHandTween = playerCardHand.transform.DOMove(_playerCardHandParent.position, _moveDuration)
                .SetEase(_moveEase);
            
            _tweenPlayerCardHand = playerCardHand;
        }

        private void HideCardHand(PlayerCardHand playerCardHand)
        {
            // Kill the previous tween if it's still active
            
            if (_cardHandTween != null && _cardHandTween.IsActive() && _tweenPlayerCardHand == playerCardHand)
                _cardHandTween.Kill();
            
            playerCardHand.transform.SetParent(_offScreenCardHandParent);
            _cardHandTween = playerCardHand.transform.DOMove(_offScreenCardHandParent.position, _moveDuration)
                .SetEase(_moveEase);
            
            _tweenPlayerCardHand = playerCardHand;
        }

        private void ShowDiceHand(PlayerDiceHand playerDiceHand)
        {
            // Kill the previous tween if it's still active
            
            if (_diceHandTween != null && _diceHandTween.IsActive() && _tweenPlayerDiceHand == playerDiceHand)
                _diceHandTween.Kill();
            
            playerDiceHand.transform.SetParent(_playerDiceHandParent);
            _diceHandTween = playerDiceHand.transform.DOMove(_playerDiceHandParent.position, _moveDuration)
                .SetEase(_moveEase);
            
            _tweenPlayerDiceHand = playerDiceHand;
        }

        private void HideDiceHand(PlayerDiceHand playerDiceHand)
        {
            // Kill the previous tween if it's still active
            
            if (_diceHandTween != null && _diceHandTween.IsActive() && _tweenPlayerDiceHand == playerDiceHand)
                _diceHandTween.Kill();
            
            playerDiceHand.transform.SetParent(_offScreenDiceHandParent);
            _diceHandTween = playerDiceHand.transform.DOMove(_offScreenDiceHandParent.position, _moveDuration)
                .SetEase(_moveEase);
            
            _tweenPlayerDiceHand = playerDiceHand;
        }
    }
}