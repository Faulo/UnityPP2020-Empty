using UnityEngine;
using UnityEngine.InputSystem;

public class RobotController : MonoBehaviour {
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

    bool canJump = true;
    int jumpCount = 0;
    float acceleration = 0;
    Vector2 footOffset = new Vector2(0, -0.5f);

    public InputAction movementAction = default;
    public InputAction jumpAction = default;
    public InputAction crouchAction = default;
    public GameObject jumpParticlesPrefab;
    public GameObject coinParticlesPrefab;

    bool wantsToJump => jumpAction.phase == InputActionPhase.Started;
    bool wantsToCrouch => crouchAction.phase == InputActionPhase.Started;

    Rigidbody2D attachedRigidbody;
    BoxCollider2D attachedCollider;

    public int currentCoinCount;
    public int maximumCoinCount;
    public int deathCount;

    void Awake() {
        attachedRigidbody = GetComponent<Rigidbody2D>();
        attachedCollider = GetComponent<BoxCollider2D>();
    }
    void Start() {
        currentCoinCount = 0;
        maximumCoinCount = GameObject.FindGameObjectsWithTag("Coin").Length;
        deathCount = 0;
    }

    void OnEnable() {
        movementAction.Enable();
        jumpAction.Enable();
        crouchAction.Enable();
    }
    void OnDisable() {
        deathCount++;
        movementAction.Disable();
        jumpAction.Disable();
        crouchAction.Disable();
    }

    void FixedUpdate() {
        var velocity = attachedRigidbody.velocity;

        intendedMovement = movementAction.ReadValue<float>();

        velocity.x = Mathf.SmoothDamp(velocity.x, intendedMovement * maximumSpeed, ref acceleration, accelerationDuration);

        if (isJumping) {
            isGrounded = false;
            if (!wantsToJump) {
                velocity.y = jumpStopSpeed;
            }
            if (velocity.y <= jumpStopSpeed) {
                isJumping = false;
            }
        }

        if (canJump && wantsToJump) {
            canJump = false;
            if (isGrounded || jumpCount > 0) {
                isJumping = true;
                jumpCount--;
                velocity.y = jumpStartSpeed;
                Instantiate(jumpParticlesPrefab, attachedRigidbody.position + footOffset, Quaternion.identity);
            }
        }
        if (!wantsToJump) {
            canJump = true;
        }

        if (isGrounded && wantsToCrouch) {
            isCrouching = true;
            attachedCollider.size = new Vector2(1, 1);
            attachedCollider.offset = footOffset;
        } else {
            isCrouching = false;
            attachedCollider.size = new Vector2(1, 2);
            attachedCollider.offset = Vector2.zero;
        }

        attachedRigidbody.velocity = velocity;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Coin")) {
            Instantiate(coinParticlesPrefab, other.transform.position, Quaternion.identity);
            Destroy(other.gameObject);
            currentCoinCount++;
        }
    }


    void OnCollisionStay2D(Collision2D collision) {
        if (CalculateContact(collision, out var contactPosition)) {
            if (contactPosition.y < transform.position.y) {
                isGrounded = true;
                jumpCount = doubleJumpCount;
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision) {
        isGrounded = false;
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
}
