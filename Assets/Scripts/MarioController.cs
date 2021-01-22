using UnityEngine;
using UnityEngine.InputSystem;

public class MarioController : MonoBehaviour, IColorable {
    public bool isGrounded = false;
    public bool isJumping = false;
    public bool isCrouching = false;

    public float defaultAcceleration = 5;
    public float maximumSpeed = 5;
    public float intendedMovement;
    public float verticalSpeed;

    public float jumpSpeed = 5;
    public float jumpForwardBoost = 1;
    public float jumpStopSpeed = 5;

    public GameObject contactParticlesPrefab;

    public InputAction movementAction = default;
    public InputAction jumpAction = default;
    public InputAction crouchAction = default;

    Rigidbody2D attachedRigidbody;
    BoxCollider2D attachedCollider;
    MarioPlatform lastPlatform;
    Renderer attachedRenderer;

    public Color groundedColor = Color.white;
    public Color jumpingColor = Color.green;
    public Color fallingColor = Color.blue;

    void Start() {
        attachedRigidbody = GetComponent<Rigidbody2D>();
        attachedCollider = GetComponent<BoxCollider2D>();
        attachedRenderer = GetComponentInChildren<Renderer>();
    }

    void OnEnable() {
        movementAction.Enable();
        jumpAction.Enable();
        crouchAction.Enable();
    }
    void OnDisable() {
        movementAction.Disable();
        jumpAction.Disable();
        crouchAction.Disable();
    }

    void Update() {
        attachedRenderer.material.color = GetCurrentColor();
    }

    public void SetColors(Color groundedColor, Color jumpingColor, Color fallingColor) {
        this.groundedColor = groundedColor;
        this.jumpingColor = jumpingColor;
        this.fallingColor = fallingColor;
    }
    public Color GetCurrentColor() {
        if (isGrounded) {
            return groundedColor;
        } else {
            if (isJumping) {
                return jumpingColor;
            } else {
                return fallingColor;
            }
        }
    }

    void FixedUpdate() {
        var velocity = attachedRigidbody.velocity;

        intendedMovement = movementAction.ReadValue<float>();

        velocity.x += intendedMovement * GetCurrentAcceleration() * Time.deltaTime;

        velocity.x = Mathf.Clamp(velocity.x, -maximumSpeed, maximumSpeed);

        if (isJumping) {
            isGrounded = false;
            if (jumpAction.phase == InputActionPhase.Waiting) {
                velocity.y = jumpStopSpeed;
            }
            if (velocity.y <= jumpStopSpeed) {
                isJumping = false;
            }
        }

        if (isGrounded && jumpAction.phase == InputActionPhase.Started) {
            isJumping = true;
            velocity.x += movementAction.ReadValue<float>() * jumpForwardBoost;
            velocity.y = GetCurrentJumpSpeed();
        }
        if (isGrounded && crouchAction.phase == InputActionPhase.Started) {
            isCrouching = true;
        } else {
            isCrouching = false;
        }

        attachedRigidbody.velocity = velocity;
    }
    void OnCollisionEnter2D(Collision2D collision) {
        if (CalculateContact(collision, out var contactPosition)) {
            // regardless, create the dust
            Instantiate(contactParticlesPrefab, contactPosition, Quaternion.identity);
        }
    }

    bool CalculateContact(Collision2D collision, out Vector2 contactPosition) {
        // let's iterate over all contact points to calculate their average
        var contactPositionSum = Vector2.zero;
        int contactPositionCount = 0;
        for (int i = 0; i < collision.contactCount; i++) {
            var contact = collision.GetContact(i);
            contactPositionSum += contact.point;
            contactPositionCount++;
        }
        if (contactPositionCount > 0) {
            // calculate the average
            contactPosition = contactPositionSum / contactPositionCount;
            return true;
        } else {
            contactPosition = Vector2.zero;
            return false;
        }
    }

    void OnCollisionStay2D(Collision2D collision) {
        if (CalculateContact(collision, out var contactPosition)) {
            if (collision.gameObject.TryGetComponent<MarioPlatform>(out var platform)) {
                if (contactPosition.y < transform.position.y) {
                    isGrounded = true;
                    // gotta save platform for later use
                    lastPlatform = platform;
                }
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision) {
        // only do stuff if we've actually hit a platform
        if (collision.gameObject.TryGetComponent<MarioPlatform>(out var platform)) {
            isGrounded = false;
        }
    }

    public float GetCurrentAcceleration() {
        if (isGrounded && lastPlatform) {
            return lastPlatform.allowedAcceleration;
        }
        return defaultAcceleration;
    }
    public float GetCurrentJumpSpeed() {
        if (isGrounded && lastPlatform) {
            return jumpSpeed * lastPlatform.jumpSpeedMultiplier;
        }
        return jumpSpeed;
    }
}
