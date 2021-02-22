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
        readonly (Object, FieldInfo)[] fieldInfos;
        public FieldBridge(GameObject obj, string name) {
            fieldInfos = FindFields(obj, name).ToArray();
            Assert.AreEqual(1, fieldInfos.Length, $"There must be exactly 1 field with the name of '{name}' in GameObject '{obj}'!");
        }
        public FieldBridge(Object obj, string name) {
            fieldInfos = FindFields(obj, name).ToArray();
            Assert.AreEqual(1, fieldInfos.Length, $"There must be exactly 1 field with the name of '{name}' in Object '{obj}'!");
        }
        IEnumerable<(Object, FieldInfo)> FindFields(GameObject obj, string name) {
            return obj.GetComponents<Component>()
                .SelectMany(component => FindFields(component, name));
        }
        IEnumerable<(Object, FieldInfo)> FindFields(Object obj, string name) {
            var fields = obj
                .GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(f => f.Name == name);
            foreach (var field in fields) {
                yield return (obj, field);
            }
        }
    }
}