using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Tests {
    public class MethodBridge<T> {
        public T Invoke(params object[] args) {
            try {
                (var component, var method) = methodInfos.First();
                return (T)method.Invoke(component, args);
            } catch (TargetInvocationException e) {
                throw e.InnerException;
            }
        }
        readonly (Object, MethodInfo)[] methodInfos;
        public MethodBridge(GameObject obj, string name, int parameterCount, string returnType) {
            methodInfos = FindMethods(obj, name).ToArray();
            Assert.AreEqual(1, methodInfos.Length, $"There must be exactly 1 method with the name of '{name}' in GameObject '{obj}'!");
            (var _, var method) = methodInfos.First();
            Assert.AreEqual(parameterCount, method.GetParameters().Length, $"The method '{name}' in GameObject '{obj}' must have exactly {parameterCount} parameters!");
            Assert.AreEqual(typeof(T), method.ReturnType, $"The method '{name}' in GameObject '{obj}' must have a return type of '{returnType}'!");
        }
        public MethodBridge(Object obj, string name, int parameterCount, string returnType) {
            methodInfos = FindMethods(obj, name).ToArray();
            Assert.AreEqual(1, methodInfos.Length, $"There must be exactly 1 method with the name of '{name}' in Object '{obj}'!");
            (var _, var method) = methodInfos.First();
            Assert.AreEqual(parameterCount, method.GetParameters().Length, $"The method '{name}' in Object '{obj}' must have exactly {parameterCount} parameters!");
            Assert.AreEqual(typeof(T), method.ReturnType, $"The method '{name}' in Object '{obj}' must have a return type of '{returnType}'!");
        }
        IEnumerable<(Object, MethodInfo)> FindMethods(GameObject obj, string name) {
            return obj.GetComponents<Component>()
                .SelectMany(component => FindMethods(component, name));
        }
        IEnumerable<(Object, MethodInfo)> FindMethods(Object obj, string name) {
            var fields = obj
                .GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(f => f.Name == name);
            foreach (var field in fields) {
                yield return (obj, field);
            }
        }
    }
    public class MethodBridge {
        public void Invoke(params object[] args) {
            try {
                (var component, var method) = methodInfos.First();
                method.Invoke(component, args);
            } catch (TargetInvocationException e) {
                throw e.InnerException;
            }
        }
        readonly (Object, MethodInfo)[] methodInfos;
        public MethodBridge(GameObject obj, string name, int parameterCount) {
            methodInfos = FindMethods(obj, name).ToArray();
            Assert.AreEqual(1, methodInfos.Length, $"There must be exactly 1 method with the name of '{name}' in GameObject '{obj}'!");
            (var _, var method) = methodInfos.First();
            Assert.AreEqual(parameterCount, method.GetParameters().Length, $"The method '{name}' in GameObject '{obj}' must have exactly {parameterCount} parameters!");
            Assert.AreEqual(typeof(void), method.ReturnType, $"The method '{name}' in GameObject '{obj}' must have a return type of 'void'!");
        }
        public MethodBridge(Object obj, string name, int parameterCount) {
            methodInfos = FindMethods(obj, name).ToArray();
            Assert.AreEqual(1, methodInfos.Length, $"There must be exactly 1 method with the name of '{name}' in Object '{obj}'!");
            (var _, var method) = methodInfos.First();
            Assert.AreEqual(parameterCount, method.GetParameters().Length, $"The method '{name}' in Object '{obj}' must have exactly {parameterCount} parameters!");
            Assert.AreEqual(typeof(void), method.ReturnType, $"The method '{name}' in Object '{obj}' must have a return type of 'void'!");
        }
        IEnumerable<(Object, MethodInfo)> FindMethods(GameObject obj, string name) {
            return obj.GetComponents<Component>()
                .SelectMany(component => FindMethods(component, name));
        }
        IEnumerable<(Object, MethodInfo)> FindMethods(Object obj, string name) {
            var fields = obj
                .GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(f => f.Name == name);
            foreach (var field in fields) {
                yield return (obj, field);
            }
        }
    }
}