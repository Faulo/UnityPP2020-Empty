using UnityEngine;
using UnityEngine.InputSystem;

public class AvatarController : MonoBehaviour
{
    public bool isGrounded = false;
    public float movementSpeed = 5;
    public float jumpSpeed = 5;
    public InputAction movementAction = default;
    public InputAction jumpAction = default;
    public Color avatarColor = default;

    private Rigidbody2D attachedRigidbody;
    private Renderer attachedRenderer;

    private void Start()
    {
        attachedRigidbody = GetComponent<Rigidbody2D>();
        attachedRenderer = GetComponentInChildren<Renderer>();
        SetRendererColor(avatarColor);
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
        Vector2 velocity = attachedRigidbody.velocity;

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
        if (collision.gameObject.TryGetComponent<ColoredPlatform>(out ColoredPlatform platform))
        {
            SetRendererColor(platform.platformColor);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }

    private void SetRendererColor(Color color)
    {
        attachedRenderer.material.SetColor("_BaseColor", color);
    }
}
