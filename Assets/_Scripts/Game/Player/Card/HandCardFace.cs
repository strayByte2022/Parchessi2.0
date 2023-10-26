using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

namespace _Scripts.Player.Card
{
    public class HandCardFace : MonoBehaviour
    {
        [SerializeField] private Transform _cardVisual;
        [SerializeField] private SortingGroup _frontSortingGroup;
        [SerializeField] private SortingGroup _backSortingGroup;

        public enum CardFaceType
        {
            Front,
            Back
        }
        
        [SerializeField] private CardFaceType _currentCardFaceType = CardFaceType.Front; 
        [SerializeField] private float _spinDuration = 0.5f;
        [SerializeField] private Ease _spinEase = Ease.OutCubic;
        private Tween _spinTween;
        
        public Tween SetCardFace(CardFaceType cardFaceType, bool isInstant = false)
        {
            if (cardFaceType == _currentCardFaceType) return null;
            
            if (_spinTween != null && _spinTween.IsActive()) _spinTween.Kill();

            if (isInstant)
            {
                SetOrder(cardFaceType);
                SetRotation(cardFaceType);
                
                _currentCardFaceType = cardFaceType;
                return null;
            }
            
            Vector3 targetRotation = cardFaceType == CardFaceType.Front ? Vector3.zero : new Vector3(0, 180, 0);

            // Create a sequence for rotation and sorting order change
            Sequence sequence = DOTween.Sequence();
            sequence.Append(_cardVisual.DORotate(new Vector3(0, 90, 0), _spinDuration / 2).SetEase(_spinEase))
                    .AppendCallback(() => SetOrder(cardFaceType))
                    .Append(_cardVisual.DORotate(targetRotation, _spinDuration / 2).SetEase(_spinEase))
                    .AppendCallback(() => SetOrder(cardFaceType));
            

            _currentCardFaceType = cardFaceType;
            _spinTween = sequence;
            return _spinTween;
        }

        private void SetOrder(CardFaceType cardFaceType)
        {
            if (cardFaceType == CardFaceType.Front)
            {
                _frontSortingGroup.sortingOrder = 1;
                _backSortingGroup.sortingOrder = 0;
            }
            else
            {
                _frontSortingGroup.sortingOrder = 0;
                _backSortingGroup.sortingOrder = 1;
            }
        }
        
        private void SetRotation(CardFaceType cardFaceType)
        {
            if (cardFaceType == CardFaceType.Front)
            {
                _cardVisual.rotation = Quaternion.identity;
            }
            else
            {
                _cardVisual.rotation = Quaternion.Euler(0, 180, 0);
            }
        }
        
        public void CompleteSpinTween()
        {
            _spinTween?.Complete();
        }
    }
}