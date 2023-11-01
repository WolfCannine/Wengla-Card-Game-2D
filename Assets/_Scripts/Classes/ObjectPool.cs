using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public int poolSize = 10;
    public Transform parentTransform;
    public GameObject poolObjectPrefab;
    private int poolObjectIndex;
    private readonly Queue<GameObject> pooledObjects = new();

    private void Awake() { for (int i = 0; i < poolSize; i++) { InstantiatePoolObject(); } }

    public GameObject GetPoolObject()
    {
        if (pooledObjects.Count == 0) { InstantiatePoolObject(); }
        GameObject obj = pooledObjects.Dequeue();
        obj.Activate();
        return obj;
    }

    public void ReturnPoolObject(GameObject obj)
    {
        obj.Deactivate();
        pooledObjects.Enqueue(obj);
        obj.transform.SetParent(gameObject.transform);
    }

    private void InstantiatePoolObject()
    {
        GameObject obj = Instantiate(poolObjectPrefab, parentTransform);
        obj.transform.SetParent(transform);
        poolObjectIndex++;
        obj.name = "Pool Object " + poolObjectIndex;
        obj.Deactivate();
        pooledObjects.Enqueue(obj);
    }
}
