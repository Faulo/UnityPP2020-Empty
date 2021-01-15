using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorController : MonoBehaviour
{
    public MarioController mario;
    public Animator animator;
    void Update()
    {
        // Der Parameter ​Speed​ soll den Betrag der aktuellen Eingabe annehmen.
        animator.SetFloat("Speed", Mathf.Abs(mario.movement));

        // Der Parameter ​Ground​ soll den Wert ​isGrounded​ aus Testat 07annehmen.
        animator.SetBool("Ground", mario.isGrounded);

        // Der Parameter ​vSpeed​ soll den Wert der vertikalenGeschwindigkeit von Mario’s Rigidbody2D annehmen.
        animator.SetFloat("vSpeed", mario.attachedRigidbody.velocity.y);

        animator.SetBool("Crouch", mario.isCrouching);
    }
}
