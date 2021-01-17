using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Tests {
    public class FieldBridge<T> {
        public T value {
            get {
                (var component, var field) = fieldInfos.First();
                return (T)field.GetValue(component);
            }
            set {
                (var component, var field) = fieldInfos.First();
                field.SetValue(component, value);
            }
        }
        readonly (Component, FieldInfo)[] fieldInfos;
        public FieldBridge(GameObject obj, string name) {
            fieldInfos = FindFields(obj, name).ToArray();
            Assert.AreEqual(1, fieldInfos.Length, $"There must be exactly 1 field with the name of '{name}' in GameObject '{obj}'!");
        }
        IEnumerable<(Component, FieldInfo)> FindFields(GameObject obj, string name) {
            foreach (var component in obj.GetComponents<Component>()) {
                var fields = component
                    .GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(f => f.Name == name);
                foreach (var field in fields) {
                    yield return (component, field);
                }
            }
        }
    }
}