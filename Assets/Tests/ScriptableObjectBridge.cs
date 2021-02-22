using UnityEngine;

namespace Tests {
    public abstract class ScriptableObjectBridge {
        public readonly ScriptableObject asset;
        public ScriptableObjectBridge(ScriptableObject asset) {
            this.asset = asset;
        }
        protected FieldBridge<T> FindField<T>(string name) {
            return new FieldBridge<T>(asset, name);
        }
        protected MethodBridge<T> FindMethod<T>(string name, int parameterCount, string returnType) {
            return new MethodBridge<T>(asset, name, parameterCount, returnType);
        }
        protected MethodBridge FindMethod(string name, int parameterCount) {
            return new MethodBridge(asset, name, parameterCount);
        }
    }
}