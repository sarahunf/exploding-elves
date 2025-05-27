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
            var go = Instantiate(prefab, transform);
            go.SetActive(false);
            var poolable = go.GetComponent<IPoolable>();
            poolable?.SetPool(this);
            return go;
        }

        public GameObject Get()
        {
            if (pool.Count == 0)
            {
                var newObj = CreateNew();
                newObj.SetActive(true);
                return newObj;
            }

            var obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        public void ReturnToPool(GameObject obj)
        {
            if (obj == null) return;
            
            obj.SetActive(false);
            pool.Enqueue(obj);
        }

        private void AddToPool(GameObject obj)
        {
            if (obj == null) return;
            pool.Enqueue(obj);
        }
    }
}