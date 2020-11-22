using System.Collections;
using System.Collections.Generic;
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
                var obj = loadedObjects.Dequeue();
                if (obj)
                {
                    Object.Destroy(obj);
                    yield return null;
                }
            }

            if (loadedScene.IsValid())
            {
                var operation = SceneManager.UnloadSceneAsync(loadedScene);
                yield return new WaitUntil(() => operation.isDone);
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

        private readonly Queue<GameObject> loadedObjects = new Queue<GameObject>();
        protected GameObject InstantiateGameObject(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            GameObject instance = Object.Instantiate(prefab, position, rotation);
            loadedObjects.Enqueue(instance);
            return instance;
        }
    }
}