using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{

    public static ObjectPool Instance;
    public int NumOfObjects;
    public GameObject ObjectToPool;
    private List<GameObject> _poolObjects;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
        }
        else
        {
            Destroy(Instance);
        }
    }

    void Start()
    {
        _poolObjects.Capacity = NumOfObjects;
        GameObject poolObject;
        for (int i = 0; i < NumOfObjects; i++)
        {
            poolObject = Instantiate(ObjectToPool, Vector3.zero, Quaternion.identity);
            poolObject.SetActive(false);
            _poolObjects.Add(poolObject);
        }
    }
    public GameObject GetObjectPool()
    {
        for (int i = 0; i < NumOfObjects; i++)
        {
            if (!_poolObjects[i].activeInHierarchy)
            {
                return _poolObjects[i];
            }
        }
        return null;
    }
}