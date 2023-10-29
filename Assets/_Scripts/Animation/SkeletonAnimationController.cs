using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class SkeletonAnimationController : MonoBehaviour
{
    private SkeletonAnimation _skeletonAnimation;

    // Start is called before the first frame update
    
    public void Initialize()
    {
        _skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
    }

    public void Flip(bool flipX)
    {
        _skeletonAnimation.skeleton.ScaleX = (flipX ? -1 : 1) * Mathf.Abs(_skeletonAnimation.skeleton.ScaleX);
    }

    public void DoIdleAnim()
    {
        _skeletonAnimation.timeScale = 1f;
        _skeletonAnimation.AnimationState.SetAnimation(0, "action/idle/normal", true);
    }

    public void DoMoveAnim()
    {
        _skeletonAnimation.timeScale = 1f;
        _skeletonAnimation.AnimationState.SetAnimation(0, "action/move-forward", true);
    }

    public void DoAttackMeleeAnim()
    {
        _skeletonAnimation.timeScale = 1f;
        _skeletonAnimation.AnimationState.SetAnimation(0, "attack/melee/tail-roll", false);
        _skeletonAnimation.AnimationState.AddAnimation(0, "action/idle/normal", true, 0.75f);
    }


    public void DoAttackRangedAnim()
    {
        _skeletonAnimation.timeScale = 1f;
        _skeletonAnimation.AnimationState.SetAnimation(0, "attack/ranged/cast-tail", false);
        _skeletonAnimation.AnimationState.AddAnimation(0, "action/idle/normal", true, 0.75f);
    }

    public void DoBuffAnim()
    {
        _skeletonAnimation.timeScale = 1f;
        _skeletonAnimation.AnimationState.SetAnimation(0, "battle/get-buff", false);
        _skeletonAnimation.AnimationState.AddAnimation(0, "action/idle/normal", true, 0.75f);
    }
    public void DoDebuffAnim()
    {
        _skeletonAnimation.timeScale = 1f;
        _skeletonAnimation.AnimationState.SetAnimation(0, "battle/get-debuff", false);
        _skeletonAnimation.AnimationState.AddAnimation(0, "action/idle/normal", true, 0.75f);
    }
    public void DoHurtAnim()
    {
        _skeletonAnimation.timeScale = 1f;
        _skeletonAnimation.AnimationState.SetAnimation(0, "defense/hit-by-normal", false);
        _skeletonAnimation.AnimationState.AddAnimation(0, "action/idle/normal", true, 0.75f);
    }
    public void DoVictoryAnim()
    {
        _skeletonAnimation.timeScale = 1f;
        _skeletonAnimation.AnimationState.SetAnimation(0, "activity/victory-pose-back-flip", false);
        _skeletonAnimation.AnimationState.AddAnimation(0, "action/idle/normal", true, 0.75f);
    }

}