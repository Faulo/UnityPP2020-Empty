using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Tests
{
    public class FieldBridge<T>
    {
        public T value
        {
            get
            {
                (Component component, FieldInfo field) = fieldInfos.First();
                return (T)field.GetValue(component);
            }
            set
            {
                (Component component, FieldInfo field) = fieldInfos.First();
                field.SetValue(component, value);
            }
        }
        private readonly (Component, FieldInfo)[] fieldInfos;
        public FieldBridge(GameObject obj, string name)
        {
            fieldInfos = FindFields(obj, name).ToArray();
            Assert.AreEqual(1, fieldInfos.Length, $"There must be exactly 1 field with the name of '{name}' in GameObject '{obj}'!");
        }
        private IEnumerable<(Component, FieldInfo)> FindFields(GameObject obj, string name)
        {
            foreach (Component component in obj.GetComponents<Component>())
            {
                IEnumerable<FieldInfo> fields = component
                    .GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(f => f.Name == name);
                foreach (FieldInfo field in fields)
                {
                    yield return (component, field);
                }
            }
        }
    }
}