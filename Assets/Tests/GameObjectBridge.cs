using System.Linq;
using UnityEngine;

namespace Tests {
    public abstract class GameObjectBridge {
        public Vector3 position {
            get => transform.position;
            set => transform.position = value;
        }
        public Vector3 scale {
            get => transform.localScale;
            set => transform.localScale = value;
        }
        public string tag {
            get => gameObject.tag;
            set => gameObject.tag = value;
        }
        public Transform transform {
            get;
            private set;
        }
        public readonly GameObject gameObject;
        public GameObjectBridge(GameObject gameObject) {
            this.gameObject = gameObject;
            transform = FindComponent<Transform>();
        }

        protected FieldBridge<T> FindField<T>(string name) {
            return new FieldBridge<T>(gameObject, name);
        }
        protected MethodBridge<T> FindMethod<T>(string name, int parameterCount, string returnType) {
            return new MethodBridge<T>(gameObject, name, parameterCount, returnType);
        }
        protected MethodBridge FindMethod(string name, int parameterCount) {
            return new MethodBridge(gameObject, name, parameterCount);
        }
        protected T FindComponent<T>()
            where T : Component {
            return gameObject.GetComponent<T>();
        }
        protected T FindComponentInChildren<T>()
            where T : Component {
            return gameObject.GetComponentInChildren<T>();
        }
        protected T[] FindComponentsInChildren<T>()
            where T : Component {
            return gameObject.GetComponentsInChildren<T>();
        }
        protected T FindInterface<T>()
            where T : class {
            return gameObject
                .GetComponents<Component>()
                .OfType<T>()
                .FirstOrDefault();
        }
        public override string ToString() => gameObject.name;
    }
}