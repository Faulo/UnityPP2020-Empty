using UnityEngine;
using UnityEngine.InputSystem;

public class RobotController : MonoBehaviour
{
    public float maximumSpeed = 5;
    public float accelerationDuration = 0;
    public float jumpStartSpeed = 5;
    public float jumpStopSpeed = 5;

    public int doubleJumpCount = 1;
    public bool isGrounded = false;
    public bool isJumping = false;
    public bool isCrouching = false;
    public float verticalSpeed => attachedRigidbody.velocity.y;
    public float intendedMovement;
    private bool canJump = true;
    private int jumpCount = 0;
    private float acceleration = 0;
    private Vector2 footOffset = new Vector2(0, -0.5f);

    public InputAction movementAction = default;
    public InputAction jumpAction = default;
    public InputAction crouchAction = default;
    public GameObject jumpParticlesPrefab;

    private bool wantsToJump => jumpAction.phase == InputActionPhase.Started;

    private bool wantsToCrouch => crouchAction.phase == InputActionPhase.Started;

    private Rigidbody2D attachedRigidbody;
    private BoxCollider2D attachedCollider;

    private void Start()
    {
        attachedRigidbody = GetComponent<Rigidbody2D>();
        attachedCollider = GetComponent<BoxCollider2D>();
    }

    private void OnEnable()
    {
        movementAction.Enable();
        jumpAction.Enable();
        crouchAction.Enable();
    }

    private void OnDisable()
    {
        movementAction.Disable();
        jumpAction.Disable();
        crouchAction.Disable();
    }

    private void FixedUpdate()
    {
        Vector2 velocity = attachedRigidbody.velocity;

        intendedMovement = movementAction.ReadValue<float>();

        velocity.x = Mathf.SmoothDamp(velocity.x, intendedMovement * maximumSpeed, ref acceleration, accelerationDuration);

        if (isJumping)
        {
            isGrounded = false;
            if (!wantsToJump)
            {
                velocity.y = jumpStopSpeed;
            }
            if (velocity.y <= jumpStopSpeed)
            {
                isJumping = false;
            }
        }

        if (canJump && wantsToJump)
        {
            canJump = false;
            if (isGrounded || jumpCount > 0)
            {
                isJumping = true;
                jumpCount--;
                velocity.y = jumpStartSpeed;
                Instantiate(jumpParticlesPrefab, attachedRigidbody.position + footOffset, Quaternion.identity);
            }
        }
        if (!wantsToJump)
        {
            canJump = true;
        }

        if (isGrounded && wantsToCrouch)
        {
            isCrouching = true;
            attachedCollider.size = new Vector2(1, 1);
            attachedCollider.offset = footOffset;
        }
        else
        {
            isCrouching = false;
            attachedCollider.size = new Vector2(1, 2);
            attachedCollider.offset = Vector2.zero;
        }

        attachedRigidbody.velocity = velocity;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (CalculateContact(collision, out Vector2 contactPosition))
        {
            if (contactPosition.y < transform.position.y)
            {
                isGrounded = true;
                jumpCount = doubleJumpCount;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }

    private bool CalculateContact(Collision2D collision, out Vector2 contactPosition)
    {
        // let's iterate over all contact points to calculate their average
        Vector2 contactPositionSum = Vector2.zero;
        int contactPositionCount = 0;
        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);
            contactPositionSum += contact.point;
            contactPositionCount++;
        }
        if (contactPositionCount > 0)
        {
            // calculate the average
            contactPosition = contactPositionSum / contactPositionCount;
            return true;
        }
        else
        {
            contactPosition = Vector2.zero;
            return false;
        }
    }
}
