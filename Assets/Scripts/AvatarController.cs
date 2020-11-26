using UnityEngine;
using UnityEngine.InputSystem;

public class AvatarController : MonoBehaviour
{
    public bool isGrounded = false;
    public float movementSpeed = 5;
    public float jumpSpeed = 5;
    public InputAction movementAction = default;
    public InputAction jumpAction = default;

    private Rigidbody2D attachedRigidbody;

    private void Start()
    {
        attachedRigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        movementAction.Enable();
        jumpAction.Enable();
    }
    private void OnDisable()
    {
        movementAction.Disable();
        jumpAction.Disable();
    }

    private void FixedUpdate()
    {
        var velocity = attachedRigidbody.velocity;

        velocity.x = movementAction.ReadValue<float>() * movementSpeed;

        if (isGrounded && jumpAction.phase == InputActionPhase.Started)
        {
            velocity.y = jumpSpeed;
        }

        attachedRigidbody.velocity = velocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }
}
