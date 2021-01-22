using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests {
    public class TestSuite {
        protected InputTestFixture input = new InputTestFixture();

        protected static Keyboard keyboard => keyboardCache ?? Keyboard.current ?? InputSystem.AddDevice<Keyboard>();
        static Keyboard keyboardCache;

        protected Scene currentScene => loadedScene.IsValid()
            ? loadedScene
            : testScene;
        Scene testScene;
        [UnitySetUp]
        public IEnumerator UnitySetUp() {
            input.Setup();
            keyboardCache = InputSystem.AddDevice<Keyboard>();
            yield return null;
            testScene = SceneManager.GetActiveScene();
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown() {
            input.TearDown();
            while (loadedObjects.Count > 0) {
                var obj = loadedObjects[0];
                loadedObjects.RemoveAt(0);
                if (obj) {
                    Object.Destroy(obj);
                    yield return null;
                }
            }

            for (var obj = FindObjectToDestroy(); obj; obj = FindObjectToDestroy()) {
                Object.Destroy(obj);
                yield return null;
            }

            if (loadedScene.IsValid()) {
                var operation = SceneManager.UnloadSceneAsync(loadedScene);
                yield return new WaitUntil(() => operation == null || operation.isDone);
                loadedScene = default;
            }
        }

        GameObject FindObjectToDestroy() {
            return SceneManager.GetActiveScene().GetRootGameObjects().Skip(1).FirstOrDefault();
        }

        Scene loadedScene;
        protected IEnumerator LoadTestScene(string name) {
            var async = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
            yield return async;
            loadedScene = SceneManager.GetSceneByName(name);
            Assert.IsTrue(loadedScene.IsValid(), $"Scene {name} could not be loaded, help!");
            yield return new WaitForFixedUpdate();
        }
        protected T LoadAsset<T>(string path) where T : Object {
            FileAssert.Exists(new FileInfo(path));
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            Assert.IsNotNull(asset, $"Could not load asset of type {typeof(T).Name} at path {path}!");
            return asset;
        }

        protected readonly List<GameObject> loadedObjects = new List<GameObject>();
        protected GameObject InstantiateGameObject(GameObject prefab, Vector3 position, Quaternion rotation) {
            var instance = Object.Instantiate(prefab, position, rotation);
            loadedObjects.Add(instance);
            return instance;
        }
        protected void DestroyGameObject(GameObject instance) {
            Assert.IsTrue(loadedObjects.Contains(instance));
            loadedObjects.Remove(instance);
            Object.Destroy(instance);
        }
        protected GameObject CreateGameObject(string name = "Temp") {
            var instance = new GameObject(name);
            loadedObjects.Add(instance);
            return instance;
        }

        protected T[] FindObjectsInScene<T>() where T : Component {
            return currentScene
                .GetRootGameObjects()
                .SelectMany(obj => obj.GetComponentsInChildren<T>())
                .ToArray();
        }
    }
}