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
    private int _originAttackValue;
    private int _originHealthValue;
    private int _originSpeedValue;
    private StylizedMapPawn _pawn;
    void Start()
    {
        _pawn = GetComponent<StylizedMapPawn>();
        _originAttackValue = _pawn.AttackDamage.Value;
        _originHealthValue = _pawn.MaxHealth.Value;
        _originSpeedValue = _pawn.MovementSpeed.Value;
        _attackText.text = _originAttackValue.ToString();
        _healthText.text = _originHealthValue.ToString();
        _speedText.text = _originSpeedValue.ToString();
        
    }

    // Update is called once per frame
    void Update()
    {
        _attackText.SetText(_pawn.AttackDamage.ToString());
        _healthText.SetText(_pawn.CurrentHealth.ToString());
        _speedText.SetText(_pawn.MovementSpeed.ToString());
    }
}
