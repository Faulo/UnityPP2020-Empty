using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

            if (loadedScene.IsValid())
            {
                AsyncOperation operation = SceneManager.UnloadSceneAsync(loadedScene);
                yield return new WaitUntil(() => operation == null || operation.isDone);
                loadedScene = default;
            }
        }
        private Scene loadedScene;
        protected Scene LoadTestScene(string name)
        {
            int sceneIndex = SceneManager.sceneCount;
            SceneManager.LoadScene(name, LoadSceneMode.Additive);
            loadedScene = SceneManager.GetSceneAt(sceneIndex);
            return loadedScene;
        }
        protected T LoadAsset<T>(string path) where T : Object
        {
            FileAssert.Exists(new FileInfo(path));
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            Assert.IsNotNull(asset, $"Could not load asset of type {typeof(T).Name} at path {path}!");
            return asset;
        }

        private readonly Queue<GameObject> loadedObjects = new Queue<GameObject>();
        protected GameObject InstantiateGameObject(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            GameObject instance = Object.Instantiate(prefab, position, rotation);
            loadedObjects.Enqueue(instance);
            return instance;
        }
    }
}