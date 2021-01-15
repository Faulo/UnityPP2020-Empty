using UnityEngine;
using UnityEngine.InputSystem;

public class MarioController : MonoBehaviour, IColorable
{
    public bool isJumping = false;
    public bool isGrounded = false;
    public float maximumSpeed = 5;
    public float jumpSpeed = 5;
    public float jumpForwardBoost = 1;
    public float jumpStopSpeed = 5;
    public float defaultAcceleration = 20;
    public InputAction movementAction = default;
    public float movement;
    public InputAction jumpAction = default;
    public InputAction crouchAction = default;
    public bool isCrouching = false;
    public GameObject contactParticlesPrefab;

    public Rigidbody2D attachedRigidbody;
    public SpriteRenderer attachedRenderer;
    private MarioPlatform lastPlatform;

    private void Start()
    {
        attachedRigidbody = GetComponent<Rigidbody2D>();
        attachedRenderer = GetComponentInChildren<SpriteRenderer>();
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
        movement = movementAction.ReadValue<float>();
        if (movement > 0)
        {
            attachedRenderer.flipX = false;
        }
        if (movement < 0)
        {
            attachedRenderer.flipX = true;
        }

        // first, store the current velocity for modification
        var velocity = attachedRigidbody.velocity;

        // acceleration means adding a difference of velocity
        velocity.x += movement * GetCurrentAcceleration() * Time.deltaTime;

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
            // Sobald die Sprungtaste gedrückt wird, soll nach wie vor Mario’s vertikaleGeschwindigkeit auf ​jumpSpeed​ gesetzt werden
            velocity.y = GetCurrentJumpSpeed();

            // Außerdem soll seine horizontale Geschwindigkeit um einen Betrag erhöhtwerden, der dem Feld ​jumpForwardBoost​ entspricht.
            velocity.x += jumpForwardBoost * movement;

            // Das Feld ​isJumping​ soll angeben, ob sich Mario gerade in derAufwärtsbewegung seines Sprunges befindet.
            isJumping = true;
        }
        if (isJumping && jumpAction.phase != InputActionPhase.Started)
        {
            // Sollte während des Sprunges die Sprungtaste losgelassen werden, sollMario’s vertikale Geschwindigkeit den Wert des Feldes ​jumpStopSpeedannehmen und ​isJumping​ soll auf ​false​ gesetzt werden.
            isJumping = false;
            velocity.y = jumpStopSpeed;
        }
        if (isJumping && velocity.y < jumpStopSpeed)
        {
            // Ansonsten soll ​isJumping​ auch dann auf ​false​ gesetzt werden, wennMario’s aktuelle vertikale Geschwindigkeit unterhalb ​jumpStopSpeedfällt.
            isJumping = false;
        }

        // if grounded, we can crouch
        if (isGrounded && crouchAction.phase == InputActionPhase.Started)
        {
            isCrouching = true;
        }
        else
        {
            isCrouching = false;
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

    private Color groundedColor = Color.white;
    private Color jumpingColor = Color.white;
    private Color fallingColor = Color.white;
    public void SetColors(Color groundedColor, Color jumpingColor, Color fallingColor)
    {
        // Die Methode ​SetColors​ legt die 3 Farben ​groundedColor​,jumpingColor​ und ​fallingColor​ fest, die Mario zur Einfärbungzur Verfügung stehen. Legen Sie für diese Farben geeignete Felderan.
        this.groundedColor = groundedColor;
        this.jumpingColor = jumpingColor;
        this.fallingColor = fallingColor;
    }

    public Color GetCurrentColor()
    {
        if (isGrounded)
        {
            // Berührt Mario gerade den Boden (​isGrounded​ ist ​true​), gibgroundedColor​ zurück.
            return groundedColor;
        }
        if (isJumping)
        {
            // Ansonsten, wenn Mario gerade springt (​isJumping​ ist ​true​),gib ​jumpingColor​ zurück.
            return jumpingColor;
        }
        // Ansonsten gib ​fallingColor​ zurück.
        return fallingColor;
    }

    private void Update()
    {
        // Implementieren Sie außerdem eine ​Update​ Methode, die die Farbe vonMario’s ​MeshRenderer​ kontinuierlich (und mittels ​GetCurrentColor​)aktualisiert.
        attachedRenderer.material.color = GetCurrentColor();
    }
}
