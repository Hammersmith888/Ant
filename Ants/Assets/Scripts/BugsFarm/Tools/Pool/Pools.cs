using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pools : MB_Singleton<Pools>
{
    [SerializeField] List<Pool> _pools;
    
    readonly Dictionary<PoolType, List<GameObject>> _objects = new Dictionary<PoolType, List<GameObject>>();

    GameObject InstantiateGO(GameObject prefab, Transform parent)
    {
        return Instantiate(prefab, Vector3.zero, Quaternion.identity, parent);
    }

    private void Start()
    {
        foreach (Pool pool in _pools)
        {
            _objects.Add(pool.type, new List<GameObject>());

            for (int i = 0; i < pool.size; i++)
                _Return(pool.type, InstantiateGO(pool.prefab, transform));
        }
    }


    public GameObject Get(PoolType type, Transform parent)
    {
        GameObject go;

        var objects = _objects[type];

        if (objects.Any())
        {
            go = objects.Last();

            go.SetActive(true);
            go.transform.SetParent(parent);

            objects.RemoveAt(objects.Count - 1);
        }
        else
        {
            Pool pool = _pools.Find(x => x.type == type);
            go = InstantiateGO(pool.prefab, parent);
        }

        return go;
    }


    void _Return(PoolType type, GameObject go)
    {
        go.SetActive(false);
        go.transform.SetParent(transform);

        _objects[type].Add(go);
    }


    public static void Return(PoolType type, GameObject go)
    {
        // Checking Pools.Instance exists, because sometimes it's destroyed BEFORE other objects at Application Quit

        Instance?._Return(type, go);
    }
}