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

    public void DoMoveAnim()
    {
        skeletonAnimation.timeScale = 1f;
        skeletonAnimation.AnimationState.SetAnimation(0, "action/move-forward", true);
    }

    public void DoAttackMeleeAnim()
    {
        skeletonAnimation.timeScale = 1f;
        skeletonAnimation.AnimationState.SetAnimation(0, "attack/melee/tail-roll", false);
        skeletonAnimation.AnimationState.AddAnimation(0, "action/idle/normal", true, 0.75f);
    }


    public void DoAttackRangedAnim()
    {
        skeletonAnimation.timeScale = 1f;
        skeletonAnimation.AnimationState.SetAnimation(0, "attack/ranged/cast-tail", false);
        skeletonAnimation.AnimationState.AddAnimation(0, "action/idle/normal", true, 0.75f);
    }

    public void DoBuffAnim()
    {
        skeletonAnimation.timeScale = 1f;
        skeletonAnimation.AnimationState.SetAnimation(0, "battle/get-buff", false);
        skeletonAnimation.AnimationState.AddAnimation(0, "action/idle/normal", true, 0.75f);
    }
    public void DoDebuffAnim()
    {
        skeletonAnimation.timeScale = 1f;
        skeletonAnimation.AnimationState.SetAnimation(0, "battle/get-debuff", false);
        skeletonAnimation.AnimationState.AddAnimation(0, "action/idle/normal", true, 0.75f);
    }
    public void DoHurtAnim()
    {
        skeletonAnimation.timeScale = 1f;
        skeletonAnimation.AnimationState.SetAnimation(0, "defense/hit-by-normal", false);
        skeletonAnimation.AnimationState.AddAnimation(0, "action/idle/normal", true, 0.75f);
    }
    public void DoVictoryAnim()
    {
        skeletonAnimation.timeScale = 1f;
        skeletonAnimation.AnimationState.SetAnimation(0, "activity/victory-pose-back-flip", false);
        skeletonAnimation.AnimationState.AddAnimation(0, "action/idle/normal", true, 0.75f);
    }
}