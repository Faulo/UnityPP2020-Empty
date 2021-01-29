using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public Transform target = default;
    public Vector2 ejectVelocity = Vector2.zero;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out Rigidbody2D other))
        {
            other.position = target.position;
            other.velocity = ejectVelocity;
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.TryGetComponent(out Rigidbody2D other))
        {
            other.position = target.position;
            other.velocity = ejectVelocity;
        }
    }
}
