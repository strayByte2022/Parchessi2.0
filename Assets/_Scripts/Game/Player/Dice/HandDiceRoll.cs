using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Player.Dice;
using DG.Tweening;
using QFSW.QC;
using UnityEngine;

public class HandDiceRoll : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rigidbody2D;
    [SerializeField] private HandDice _handDice;
    [SerializeField] private HandDiceDragAndTargeter _handDiceDragAndTargeter;
    [SerializeField] private Animator _animator;
    private static readonly int EndNumber = Animator.StringToHash("END_NUMBER");

    [SerializeField] private float _force = 100f;
    [SerializeField] private float _torque = 100f;
    [SerializeField] private float _rollDuration = 2.0f; // The duration of the roll animation in seconds
    [SerializeField] private Ease _rollEase = Ease.OutQuad; // Adjust the ease function as needed

    private bool _isRolling = false;
    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _handDice = GetComponentInParent<HandDice>();
        _handDice.DiceValue.OnChangeValue += SetEndNumber;
    }

    public void PlayRollAnimation()
    {
        _animator.SetInteger(EndNumber, 0);
    }

    public void SetEndNumber(int beforeNumber, int endNumber)
    {
        _animator.SetInteger(EndNumber, endNumber);
    }
    
    private IEnumerator PlayRollAnimationCoroutine()
    {
        _animator.SetInteger(EndNumber, 0);
        yield return new WaitForSeconds(0.5f);
        _animator.SetInteger(EndNumber, _handDice.DiceValue.Value);
        yield return new WaitForSeconds(0.5f);
        
    }
    
    [Command]
    public void RollDice()
    {
        if (_isRolling)
            return;

        _handDiceDragAndTargeter.DisableDrag();

        var randomX = UnityEngine.Random.Range(-1f, 1f);
        var randomY = UnityEngine.Random.Range(-1f, 1f);
        var randomVector = new Vector2(randomX, randomY);
        _rigidbody2D.velocity = randomVector * _force;

        var randomTorque = UnityEngine.Random.Range(-1f, 1f);
        _rigidbody2D.angularVelocity = randomTorque * _torque;
        
        float timeScale = 1;
        var initialAngularVelocity = _rigidbody2D.angularVelocity;
        var initialVelocity = _rigidbody2D.velocity;
        
        DOTween.To(() => timeScale, x => timeScale = x, 0.0f, _rollDuration)
            .SetEase(_rollEase)
            .OnUpdate(() =>
            {
                _rigidbody2D.angularVelocity = initialAngularVelocity * timeScale;
                _rigidbody2D.velocity = initialVelocity * timeScale;
            })
            .OnComplete(() => {
                // Animation is complete
                _handDiceDragAndTargeter.EnableDrag();
                _isRolling = false;
            });
        _isRolling = true;
    }

}
