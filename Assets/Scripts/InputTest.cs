using UnityEngine;
using UnityEngine.InputSystem;

public class InputTest : MonoBehaviour
{
    public float movementSpeed = 5;
    private void Start()
    {

    }

    private void FixedUpdate()
    {
        var direction = Vector3.zero;
        if (Keyboard.current.upArrowKey.isPressed)
        {
            direction += Vector3.up;
        }
        if (Keyboard.current.downArrowKey.isPressed)
        {
            direction += Vector3.down;
        }
        if (Keyboard.current.leftArrowKey.isPressed)
        {
            direction += Vector3.left;
        }
        if (Keyboard.current.rightArrowKey.isPressed)
        {
            direction += Vector3.right;
        }
        if (direction != Vector3.zero)
        {
            transform.position += direction.normalized * movementSpeed * Time.deltaTime;
        }
    }
}
