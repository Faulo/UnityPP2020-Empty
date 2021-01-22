using UnityEngine;

public class Teleporter : MonoBehaviour {
    public Transform target = default;

    // Variante 1: Teleporter-Collider ist NICHT "Trigger"
    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.TryGetComponent(out Rigidbody2D other)) {
            other.position = target.position;
        }
    }

    // Variante 2: Teleporter-Collider IST "Trigger"
    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.TryGetComponent(out Rigidbody2D other)) {
            other.position = target.position;
        }
    }
}
