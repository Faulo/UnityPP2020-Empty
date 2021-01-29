using UnityEngine;
using UnityEngine.InputSystem;

public class RobotController : MonoBehaviour
{
    public bool isGrounded = false;
    public bool isJumping = false;
    public bool isCrouching = false;

    public float maximumSpeed = 5;
    public float intendedMovement;
    public float verticalSpeed;

    public float jumpStartSpeed = 5;
    public float jumpStopSpeed = 5;

    public InputAction movementAction = default;
    public InputAction jumpAction = default;
    public InputAction crouchAction = default;

    private Rigidbody2D attachedRigidbody;
    private BoxCollider2D attachedCollider;
    private Renderer attachedRenderer;

    private void Start()
    {
        attachedRigidbody = GetComponent<Rigidbody2D>();
        attachedCollider = GetComponent<BoxCollider2D>();
        attachedRenderer = GetComponentInChildren<Renderer>();
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

    private void Update()
    {
    }


    private void FixedUpdate()
    {
        Vector2 velocity = attachedRigidbody.velocity;

        intendedMovement = movementAction.ReadValue<float>();

        // velocity.x += intendedMovement * GetCurrentAcceleration() * Time.deltaTime;

        velocity.x = Mathf.Clamp(velocity.x, -maximumSpeed, maximumSpeed);

        if (isJumping)
        {
            isGrounded = false;
            if (jumpAction.phase == InputActionPhase.Waiting)
            {
                velocity.y = jumpStopSpeed;
            }
            if (velocity.y <= jumpStopSpeed)
            {
                isJumping = false;
            }
        }

        if (isGrounded && jumpAction.phase == InputActionPhase.Started)
        {
            isJumping = true;
            velocity.y = jumpStartSpeed;
        }
        if (isGrounded && crouchAction.phase == InputActionPhase.Started)
        {
            isCrouching = true;
        }
        else
        {
            isCrouching = false;
        }

        attachedRigidbody.velocity = velocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
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

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (CalculateContact(collision, out Vector2 contactPosition))
        {
            if (contactPosition.y < transform.position.y)
            {
                isGrounded = true;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }
}
