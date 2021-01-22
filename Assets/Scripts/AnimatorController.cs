using System;
using UnityEngine;

public class AnimatorController : MonoBehaviour
{
    SpriteRenderer attachedRenderer;
    Animator attachedAnimator;
    MarioController attachedMario;

    void Start()
    {
        attachedRenderer = GetComponent<SpriteRenderer>();
        attachedAnimator = GetComponent<Animator>();
        attachedMario = GetComponentInParent<MarioController>();
    }

    void Update()
    {
        switch (Math.Sign(attachedMario.intendedMovement))
        {
            case 1:
                attachedRenderer.flipX = false;
                break;
            case -1:
                attachedRenderer.flipX = true;
                break;
        }
        attachedAnimator.SetBool("Ground", attachedMario.isGrounded);
        attachedAnimator.SetBool("Crouch", attachedMario.isCrouching);
        attachedAnimator.SetFloat("Speed", Mathf.Abs(attachedMario.intendedMovement));
        attachedAnimator.SetFloat("vSpeed", attachedMario.verticalSpeed);
    }
}
