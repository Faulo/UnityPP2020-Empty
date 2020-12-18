using UnityEngine;
using UnityEngine.InputSystem;

public class MarioController : MonoBehaviour
{
    public bool isGrounded = false;
    public float maximumSpeed = 5;
    public float jumpSpeed = 5;
    public float defaultAcceleration = 20;
    public InputAction movementAction = default;
    public InputAction jumpAction = default;
    public GameObject contactParticlesPrefab;

    private Rigidbody2D attachedRigidbody;
    private MarioPlatform lastPlatform;

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
        // first, store the current velocity for modification
        var velocity = attachedRigidbody.velocity;

        // acceleration means adding a difference of velocity
        velocity.x += movementAction.ReadValue<float>() * GetCurrentAcceleration() * Time.deltaTime;

        // velocity must not exceed maximumSpeed
        if (velocity.x > maximumSpeed)
        {
            velocity.x = maximumSpeed;
        }
        if (velocity.x < -maximumSpeed)
        {
            velocity.x = -maximumSpeed;
        }

        // if grounded, we can jump
        if (isGrounded && jumpAction.phase == InputActionPhase.Started)
        {
            velocity.y = GetCurrentJumpSpeed();
        }

        // write velocity back to rigidbody
        attachedRigidbody.velocity = velocity;
    }

    private float GetCurrentAcceleration()
    {
        // if we're grounded, use platform's allowedAcceleration
        if (isGrounded)
        {
            return lastPlatform.allowedAcceleration;
        }
        else
        {
            return defaultAcceleration;
        }
    }

    private float GetCurrentJumpSpeed()
    {
        // if we're grounded, use platform's jumpSpeedMultiplier
        if (isGrounded)
        {
            return jumpSpeed * lastPlatform.jumpSpeedMultiplier;
        }
        else
        {
            return jumpSpeed;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // only do stuff if we've collided with a platform
        if (collision.gameObject.TryGetComponent<MarioPlatform>(out var platform))
        {
            // calculate collision point average
            Vector2 sum = new Vector2();
            for (int i = 0; i < collision.contactCount; i++)
            {
                sum += collision.GetContact(i).point;
            }
            Vector2 average = sum / collision.contactCount;

            // only become grounded if platform is beneath us
            if (average.y < transform.position.y)
            {
                lastPlatform = platform;
                isGrounded = true;
            }

            // regardless, spawn particles at collisioin point
            Instantiate(contactParticlesPrefab, average, Quaternion.identity);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // only do stuff if we've collided with a platform
        if (collision.gameObject.TryGetComponent<MarioPlatform>(out var platform))
        {
            isGrounded = false;
        }
    }
}
