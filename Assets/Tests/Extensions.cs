using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Extensions
{
    public static IEnumerable<GameObject> GetPrefabInstances(this Scene scene, GameObject prefab)
    {
        return scene.GetRootGameObjects()
            .SelectMany(obj => obj.GetComponentsInChildren<Transform>())
            .Select(t => t.gameObject)
            .Where(obj => obj.name.StartsWith(prefab.name));
    }
    public static IEnumerable<GameObject> GetObjectsByName(this Scene scene, string name)
    {
        return scene.GetRootGameObjects()
            .SelectMany(obj => obj.GetComponentsInChildren<Transform>())
            .Select(t => t.gameObject)
            .Where(obj => obj.name == name);
    }
}
