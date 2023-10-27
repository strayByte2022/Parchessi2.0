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
    private int _endNumber = 0;
    public Action<int> OnRollComplete {get; set;}
    
    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _handDice = GetComponentInParent<HandDice>();
        _handDice.DiceValue.OnChangeValue += SetEndNumber;
    }

    private void PlayRollAnimation()
    {
        _animator.SetInteger(EndNumber, 0);
    }

    private void SetEndNumber(int beforeNumber, int endNumber)
    {
        if (endNumber != 0)
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
    public void RollDice(int endNumber, Action<int> callback = null)
    {
        if (_isRolling)
            return;

        _handDiceDragAndTargeter.DisableDrag();
        _rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        
        var randomX = UnityEngine.Random.Range(-1f, 1f);
        var randomY = UnityEngine.Random.Range(-1f, 1f);
        var randomVector = new Vector2(randomX, randomY);
        _rigidbody2D.AddForce(randomVector * _force);

        var randomTorque = UnityEngine.Random.Range(-1f, 1f);
        _rigidbody2D.AddTorque(randomTorque * _torque);

        _isRolling = true;
        _endNumber = endNumber;
        OnRollComplete += callback;
    }

    public void FixedUpdate()
    {
        if (_isRolling)
        {
            if (_rigidbody2D.velocity.magnitude < 1f)
            {
                _rigidbody2D.velocity = Vector2.zero;
                _rigidbody2D.angularVelocity = 0;
                CompleteRoll();
                
            }
            
        }
    }

    private void CompleteRoll()
    {
        PlayRollAnimation();
        _rigidbody2D.velocity = Vector2.zero;
        _rigidbody2D.angularVelocity = 0;
        
        _rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        _handDiceDragAndTargeter.EnableDrag();
        _isRolling = false;

        SetEndNumber(0,_endNumber);
        
        OnRollComplete?.Invoke(_endNumber);
    }
}