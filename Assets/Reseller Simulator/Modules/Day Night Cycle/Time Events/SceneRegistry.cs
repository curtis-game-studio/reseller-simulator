using System.Collections.Generic;
using UnityEngine;

public class SceneRegistry : MonoBehaviour
{
    public static SceneRegistry Instance;

    private Dictionary<string, GameObject> registry = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Register(string id, GameObject obj)
    {
        if (!string.IsNullOrEmpty(id))
        {
            registry[id] = obj;
        }
    }

    public GameObject Get(string id)
    {
        registry.TryGetValue(id, out var obj);
        return obj;
    }
}
