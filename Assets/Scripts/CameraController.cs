using UnityEngine;

public class CameraController : MonoBehaviour {
    public Transform target;
    public float distance = 10;
    public float moveDuration = 1;
    public Vector3 velocity;

    void FixedUpdate() {
        var position = target.position;
        position.z = -distance;
        transform.position = Vector3.SmoothDamp(transform.position, position, ref velocity, moveDuration);
    }
}
