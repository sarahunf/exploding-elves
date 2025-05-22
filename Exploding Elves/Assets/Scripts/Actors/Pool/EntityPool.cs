using System.Collections.Generic;
using UnityEngine;
using Actors.Interface;

namespace Actors.Pool
{
    public class EntityPool : MonoBehaviour, IPool
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int initialSize = 10;

        private readonly Queue<GameObject> pool = new();

        public void Initialize(GameObject prefab, int size)
        {
            this.prefab = prefab;
            this.initialSize = size;
            
            Debug.Log($"[{gameObject.name}] Initializing pool with {initialSize} objects");
            for (int i = 0; i < initialSize; i++)
            {
                AddToPool(CreateNew());
            }
        }

        private void Awake()
        {
            if (prefab != null)
            {
                Debug.Log($"[{gameObject.name}] Initializing pool with {initialSize} objects");
                for (int i = 0; i < initialSize; i++)
                {
                    AddToPool(CreateNew());
                }
            }
        }

        private GameObject CreateNew()
        {
            Debug.Log($"[{gameObject.name}] Creating new object in pool");
            GameObject go = Instantiate(prefab, transform);
            go.SetActive(false);
            go.GetComponent<IPoolable>()?.SetPool(this);
            return go;
        }

        public GameObject Get()
        {
            if (pool.Count == 0)
            {
                Debug.LogWarning($"[{gameObject.name}] Pool is empty! Creating new object");
                AddToPool(CreateNew());
            }

            var obj = pool.Dequeue();
            Debug.Log($"[{gameObject.name}] Getting object from pool. Remaining: {pool.Count}");
            obj.SetActive(true);
            return obj;
        }

        public void ReturnToPool(GameObject obj)
        {
            Debug.Log($"[{gameObject.name}] Returning object to pool. Current size: {pool.Count}");
            obj.SetActive(false);
            pool.Enqueue(obj);
        }

        private void AddToPool(GameObject obj)
        {
            pool.Enqueue(obj);
        }
    }
}