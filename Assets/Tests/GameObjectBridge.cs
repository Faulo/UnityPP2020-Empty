using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public abstract class GameObjectBridge
    {
        public Vector3 position
        {
            get => transform.position;
            set => transform.position = value;
        }
        public Vector3 scale
        {
            get => transform.localScale;
            set => transform.localScale = value;
        }
        public Transform transform
        {
            get;
            private set;
        }
        public readonly GameObject gameObject;
        public GameObjectBridge(GameObject gameObject)
        {
            this.gameObject = gameObject;
            transform = FindComponent<Transform>();
        }

        protected FieldBridge<T> FindField<T>(string name)
        {
            return new FieldBridge<T>(gameObject, name);
        }
        protected MethodBridge<T> FindMethod<T>(string name, int parameterCount, string returnType)
        {
            return new MethodBridge<T>(gameObject, name, parameterCount, returnType);
        }
        protected T FindComponent<T>() where T : Component
        {
            T[] components = gameObject.GetComponents<T>();
            Assert.AreEqual(1, components.Length, $"There must be exactly 1 component of type of type '{typeof(T)}' in GameObject '{gameObject}'!");
            return components[0];
        }
        protected T FindComponentInChildren<T>() where T : Component
        {
            T[] components = gameObject.GetComponentsInChildren<T>();
            Assert.AreEqual(1, components.Length, $"There must be exactly 1 component of type of type '{typeof(T)}' in GameObject '{gameObject}' (or its children)!");
            return components[0];
        }
    }
}