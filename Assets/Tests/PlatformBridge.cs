using UnityEngine;

namespace Tests
{
    public class PlatformBridge : GameObjectBridge
    {
        public BoxCollider2D collider
        {
            get;
            private set;
        }

        public PlatformBridge(GameObject gameObject) : base(gameObject)
        {
            collider = FindComponent<BoxCollider2D>();
        }
    }
}