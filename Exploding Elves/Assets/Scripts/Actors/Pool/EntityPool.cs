using System.Collections.Generic;
using UnityEngine;

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
            
            for (int i = 0; i < initialSize; i++)
            {
                AddToPool(CreateNew());
            }
        }

        private void Awake()
        {
            if (prefab != null)
            {
                for (int i = 0; i < initialSize; i++)
                {
                    AddToPool(CreateNew());
                }
            }
        }

        private GameObject CreateNew()
        {
            GameObject go = Instantiate(prefab, transform);
            go.SetActive(false);
            go.GetComponent<IPoolable>()?.SetPool(this);
            go.SetActive(true);
            return go;
        }

        public GameObject Get()
        {
            if (pool.Count == 0)
            {
                Debug.LogWarning($"[{gameObject.name}] Pool is empty! Creating new object");
                return CreateNew();
            }

            var obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        public void ReturnToPool(GameObject obj)
        {
            obj.SetActive(false);
            pool.Enqueue(obj);
        }

        private void AddToPool(GameObject obj)
        {
            pool.Enqueue(obj);
        }
    }
}