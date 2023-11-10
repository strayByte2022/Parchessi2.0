using _Scripts.Player.Pawn;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PawnVisualScript : MonoBehaviour
{
    [SerializeField] private TMP_Text _attackText;
    [SerializeField] private TMP_Text _healthText;
    [SerializeField] private TMP_Text _speedText;

    [SerializeField] private Color _buffColor = Color.green;
    [SerializeField] private Color _debuffColor = Color.red;

    private bool _isInitialized;
    private int _originAttackValue;
    private int _originHealthValue;
    private int _originSpeedValue;
    private StylizedMapPawn _pawn;

    void Start()
    {
        _pawn = GetComponent<StylizedMapPawn>();

        _pawn.AttackDamage.OnChangeValue += UpdateAttack;
        _pawn.MaxHealth.OnChangeValue += UpdateHealth;
        _pawn.MovementSpeed.OnChangeValue += UpdateSpeed;

        
        _originAttackValue = _pawn.AttackDamage.Value;
        _originHealthValue = _pawn.MaxHealth.Value;
        _originSpeedValue = _pawn.MovementSpeed.Value;
        _attackText.text = _originAttackValue.ToString();
        _healthText.text = _originHealthValue.ToString();
        _speedText.text = _originSpeedValue.ToString();
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