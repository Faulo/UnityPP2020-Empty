using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Tests
{
    public class MethodBridge<T>
    {
        public T Invoke(params object[] args)
        {
            var (component, method) = methodInfos.First();
            return (T)method.Invoke(component, args);
        }
        private readonly (Component, MethodInfo)[] methodInfos;
        public MethodBridge(GameObject obj, string name)
        {
            methodInfos = FindMethods(obj, name).ToArray();
            Assert.AreEqual(1, methodInfos.Length, $"There must be exactly 1 method with the name of '{name}' in GameObject '{obj}'!");
        }
        private IEnumerable<(Component, MethodInfo)> FindMethods(GameObject obj, string name)
        {
            foreach (var component in obj.GetComponents<Component>())
            {
                var fields = component
                    .GetType()
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(f => f.Name == name);
                foreach (var field in fields)
                {
                    yield return (component, field);
                }
            }
        }
    }
}