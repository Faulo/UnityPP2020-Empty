using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests
{
    public class TestSuite
    {
        protected Scene currentScene => loadedScene.IsValid()
            ? loadedScene
            : testScene;
        private Scene testScene;
        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            yield return null;
            testScene = SceneManager.GetActiveScene();
        }

        [UnityTearDown]
        public IEnumerator UnityTearDown()
        {
            while (loadedObjects.Count > 0)
            {
                GameObject obj = loadedObjects.Dequeue();
                if (obj)
                {
                    Object.Destroy(obj);
                    yield return null;
                }
            }

            for (GameObject obj = FindObjectToDestroy(); obj; obj = FindObjectToDestroy())
            {
                Object.Destroy(obj);
                yield return null;
            }

            if (loadedScene.IsValid())
            {
                AsyncOperation operation = SceneManager.UnloadSceneAsync(loadedScene);
                yield return new WaitUntil(() => operation == null || operation.isDone);
                loadedScene = default;
            }
        }

        private GameObject FindObjectToDestroy()
        {
            return SceneManager.GetActiveScene().GetRootGameObjects().Skip(1).FirstOrDefault();
        }

        private Scene loadedScene;
        protected IEnumerator LoadTestScene(string name)
        {
            AsyncOperation async = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
            yield return async;
            loadedScene = SceneManager.GetSceneByName(name);
            Assert.IsTrue(loadedScene.IsValid(), $"Scene {name} could not be loaded, help!");
            yield return new WaitForFixedUpdate();
        }
        protected T LoadAsset<T>(string path) where T : Object
        {
            FileAssert.Exists(new FileInfo(path));
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            Assert.IsNotNull(asset, $"Could not load asset of type {typeof(T).Name} at path {path}!");
            return asset;
        }

        protected readonly Queue<GameObject> loadedObjects = new Queue<GameObject>();
        protected GameObject InstantiateGameObject(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            GameObject instance = Object.Instantiate(prefab, position, rotation);
            loadedObjects.Enqueue(instance);
            return instance;
        }
    }
}