using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class SkeletonAnimationController : MonoBehaviour
{
    private SkeletonAnimation skeletonAnimation;

    // Start is called before the first frame update
    void Awake()
    {
        skeletonAnimation = this.GetComponent<SkeletonAnimation>();
    }

    private void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //for debug only
        if (skeletonAnimation != null)
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                DoJumpAnim();
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                DoAttackMeleeAnim();
            }
            
            if (Input.GetKeyDown(KeyCode.R))
            {
                DoAttackRangedAnim();
            }
            
            if (Input.GetKeyDown(KeyCode.B))
            {
                DoBuffAnim();
            }
            
            if (Input.GetKeyDown(KeyCode.I))
            {
                DoIdleAnim();
            }
        }
    }

    public void Flip(bool flipX)
    {
        skeletonAnimation.skeleton.ScaleX = (flipX ? -1 : 1) * Mathf.Abs(skeletonAnimation.skeleton.ScaleX);
    }

    public void DoIdleAnim()
    {
        skeletonAnimation.timeScale = 1f;
        skeletonAnimation.AnimationState.SetAnimation(0, "action/idle/normal", true);
    }

    public void DoJumpAnim()
    {
        skeletonAnimation.timeScale = 1f;
        skeletonAnimation.AnimationState.SetAnimation(0, "action/move-forward", false);
    }

    public void DoAttackMeleeAnim()
    {
        skeletonAnimation.timeScale = 1f;
        skeletonAnimation.AnimationState.SetAnimation(0, "attack/melee/tail-roll", false);
    }


    public void DoAttackRangedAnim()
    {
        skeletonAnimation.timeScale = 1f;
        skeletonAnimation.AnimationState.SetAnimation(0, "attack/ranged/tail-roll", false);
    }

    public void DoBuffAnim()
    {
        skeletonAnimation.timeScale = 1f;
        skeletonAnimation.AnimationState.SetAnimation(0, "attack/ranged/tail-roll", false);
    }
}