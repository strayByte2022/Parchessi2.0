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

        private void Awake()
        {
            SetCardFace(CardFaceType.Back);
            CompleteSpinTween();
        }

        public void SetCardFace(CardFaceType cardFaceType)
        {
            if (cardFaceType == _currentCardFaceType) return;

            if (_spinTween != null && _spinTween.IsActive()) _spinTween.Kill();

            Vector3 targetRotation = cardFaceType == CardFaceType.Front ? Vector3.zero : new Vector3(0, 180, 0);

            // Create a sequence for rotation and sorting order change
            Sequence sequence = DOTween.Sequence();
            sequence.Append(_cardVisual.DORotate(targetRotation, _spinDuration / 2)
                    .SetEase(_spinEase)
                    .OnKill(() =>
                    {
                        // Update the sorting order at 90 degrees
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
                    }))
                .Append(_cardVisual.DORotate(targetRotation, _spinDuration / 2).SetEase(_spinEase))
                .OnComplete(() =>
                {
                    // Update the current card face type
                    _currentCardFaceType = cardFaceType;
                });

            _spinTween = sequence;
        }
        
        public Tween GetSpinTween(CardFaceType cardFaceType)
        {
            SetCardFace(cardFaceType);
            return _spinTween;
        }
        
        public void CompleteSpinTween()
        {
            _spinTween?.Complete();
        }
    }
}