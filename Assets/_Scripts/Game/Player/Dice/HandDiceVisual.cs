using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandDiceVisual : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    private static readonly int EndNumber = Animator.StringToHash("END_NUMBER");

    
    public void PlayRollAnimation()
    {
        _animator.SetInteger(EndNumber, 0);
    }

    public void SetEndNumber(int endNumber)
    {
        _animator.SetInteger(EndNumber, endNumber);
    }
}
