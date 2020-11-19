using UnityEngine;
using UnityEngine.InputSystem;

namespace Tests
{
    public class AvatarBridge : GameObjectBridge
    {
        public bool isGrounded
        {
            get => isGroundedBridge.value;
            set => isGroundedBridge.value = value;
        }
        private readonly FieldBridge<bool> isGroundedBridge;

        public float movementSpeed
        {
            get => movementSpeedBridge.value;
            set => movementSpeedBridge.value = value;
        }
        private readonly FieldBridge<float> movementSpeedBridge;

        public float jumpSpeed
        {
            get => jumpSpeedBridge.value;
            set => jumpSpeedBridge.value = value;
        }
        private readonly FieldBridge<float> jumpSpeedBridge;

        public InputAction movementAction => movementActionBridge.value;
        private readonly FieldBridge<InputAction> movementActionBridge;
        public InputAction jumpAction => jumpActionBridge.value;
        private readonly FieldBridge<InputAction> jumpActionBridge;

        public Rigidbody2D rigidbody
        {
            get;
            private set;
        }
        public BoxCollider2D collider
        {
            get;
            private set;
        }

        public AvatarBridge(GameObject gameObject) : base(gameObject)
        {
            isGroundedBridge = FindField<bool>(nameof(isGrounded));
            movementSpeedBridge = FindField<float>(nameof(movementSpeed));
            jumpSpeedBridge = FindField<float>(nameof(jumpSpeed));
            movementActionBridge = FindField<InputAction>(nameof(movementAction));
            jumpActionBridge = FindField<InputAction>(nameof(jumpAction));
            rigidbody = FindComponent<Rigidbody2D>();
            collider = FindComponent<BoxCollider2D>();
        }
    }
}