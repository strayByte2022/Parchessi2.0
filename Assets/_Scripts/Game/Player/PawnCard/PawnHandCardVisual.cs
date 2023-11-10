using _Scripts.Scriptable_Objects;
using TMPro;
using UnityEngine;

namespace _Scripts.Player.PawnCard
{
    [RequireComponent(typeof(PawnHandCard))]
    public class PawnHandCardVisual : HandCardVisual
    {
        private PawnHandCard _pawnHandCard;
        
        [SerializeField] private TMP_Text _attackText;
        [SerializeField] private TMP_Text _healthText;
        [SerializeField] private TMP_Text _speedText;

        [SerializeField] private Color _buffColor = Color.green;
        [SerializeField] private Color _debuffColor = Color.red;

        private bool _isInitialized;
        private int _originAttackValue;
        private int _originHealthValue;
        private int _originSpeedValue;
        
        protected override void Awake()
        {
            base.Awake();

            _pawnHandCard = GetComponent<PawnHandCard>();
            
            _pawnHandCard.Attack.OnChangeValue += UpdateAttack;
            _pawnHandCard.MaxHealth.OnChangeValue += UpdateHealth;
            _pawnHandCard.Speed.OnChangeValue += UpdateSpeed;
            
        }
        
        
        
        protected override void LoadCard()
        {
            if(CardDescription == null) return;
            base.LoadCard();
            
            _cardImage.sprite = _pawnHandCard.PawnDescription.PawnSprite;
            
            _originAttackValue = _pawnHandCard.PawnDescription.PawnAttackDamage;
            _originHealthValue = _pawnHandCard.PawnDescription.PawnMaxHealth;
            _originSpeedValue = _pawnHandCard.PawnDescription.PawnMovementSpeed;

            _isInitialized = true;
        }

        protected void UpdateAttack(int oldValue, int newValue)
        {
            
            _attackText.text = newValue.ToString();
            
            if (!_isInitialized) return;
            
            if (newValue > _originAttackValue)
            {
                _attackText.color = _buffColor;
            }
            else if (newValue < _originAttackValue)
            {
                _attackText.color = _debuffColor;
            }
            else
            {
                _attackText.color = Color.white;
            }
            
        }

        protected void UpdateHealth(int oldValue, int newValue)
        {
            
            _healthText.text = newValue.ToString();
            
            if (!_isInitialized) return;
            
            if (newValue > _originHealthValue)
            {
                _healthText.color = _buffColor;
            }
            else if (newValue < _originHealthValue)
            {
                _healthText.color = _debuffColor;
            }
            else
            {
                _healthText.color = Color.white;
            }
            
        }
        
        protected void UpdateSpeed(int oldValue, int newValue)
        {
            _speedText.text = newValue.ToString();
            
            if (!_isInitialized) return;
            
            if (newValue > _originSpeedValue)
            {
                _speedText.color = _buffColor;
            }
            else if (newValue < _originSpeedValue)
            {
                _speedText.color = _debuffColor;
            }
            else
            {
                _speedText.color = Color.white;
            }
            
        }
        
    
    }
}