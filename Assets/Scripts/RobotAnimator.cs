using System;
using UnityEngine;

public class RobotAnimator : MonoBehaviour
{
    private SpriteRenderer attachedRenderer;
    private Animator attachedAnimator;
    private RobotController attachedRobot;

    private void Start()
    {
        attachedRenderer = GetComponent<SpriteRenderer>();
        attachedAnimator = GetComponent<Animator>();
        attachedRobot = GetComponentInParent<RobotController>();
    }

    private void Update()
    {
        switch (Math.Sign(attachedRobot.intendedMovement))
        {
            case 1:
                attachedRenderer.flipX = false;
                break;
            case -1:
                attachedRenderer.flipX = true;
                break;
        }
        attachedAnimator.SetBool("Ground", attachedRobot.isGrounded);
        attachedAnimator.SetBool("Crouch", attachedRobot.isCrouching);
        attachedAnimator.SetBool("Jump", attachedRobot.isJumping);
        attachedAnimator.SetFloat("Speed", Mathf.Abs(attachedRobot.intendedMovement));
        attachedAnimator.SetFloat("vSpeed", attachedRobot.verticalSpeed);
    }
}
