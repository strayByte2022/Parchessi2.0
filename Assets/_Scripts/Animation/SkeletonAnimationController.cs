using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
public class SkeletonAnimationController : MonoBehaviour
{
    [SerializeField]
    [SpineAnimation]
    string _animRun;
    [SerializeField]
    [SpineAnimation]
    string _animAttack;
    [SerializeField]
    [SpineAnimation]
    string _animIdle;
    private SkeletonAnimation _animator;
    // Start is called before the first frame update
    void Start()
    {
        _animator = this.GetComponent<SkeletonAnimation>();
        _animator.AnimationState.SetAnimation(0, _animIdle, true);
    }

    // Update is called once per frame
    void Update()
    {
        //for debug only
        if (_animator != null)
        {
            if(Input.GetKeyDown(KeyCode.R))
            {
                Run();
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                Attack();
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                Idle();
            }
        }

    }
    public void Run()
    {
        _animator.AnimationState.SetAnimation(0, _animRun, true);
    }
    public void Attack()
    {
        _animator.AnimationState.SetAnimation(0,_animAttack, true);
    }
    public void Idle()
    {
        _animator.AnimationState.SetAnimation(0, _animIdle, true);
    }

}
