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
    private StylizedMapPawn pawn;
    void Start()
    {
        pawn = GetComponent<StylizedMapPawn>();
        _originAttackValue = pawn.AttackDamage.Value;
        _originHealthValue = pawn.MaxHealth.Value;
        _originSpeedValue = pawn.MovementSpeed.Value;
        _attackText.SetText(_originAttackValue.ToString());
        _healthText.SetText(_originHealthValue.ToString());
        _speedText.SetText(_originSpeedValue.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        _attackText.SetText(pawn.AttackDamage.ToString());
        _healthText.SetText(pawn.CurrentHealth.ToString());
        _speedText.SetText(pawn.MovementSpeed.ToString());
    }
}
