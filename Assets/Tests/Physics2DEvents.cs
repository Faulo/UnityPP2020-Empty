using System;
using UnityEngine;


namespace Tests
{
    public class Physics2DEvents : MonoBehaviour
    {
        public event Action<Collision2D> onCollisionEnter;
        public event Action<Collision2D> onCollisionStay;
        public event Action<Collision2D> onCollisionExit;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            onCollisionEnter?.Invoke(collision);
        }
        private void OnCollisionStay2D(Collision2D collision)
        {
            onCollisionStay?.Invoke(collision);
        }
        private void OnCollisionExit2D(Collision2D collision)
        {
            onCollisionExit?.Invoke(collision);
        }
    }
}