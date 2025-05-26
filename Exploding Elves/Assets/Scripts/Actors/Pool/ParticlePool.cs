using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actors.Pool
{
    public class ParticlePool : MonoBehaviour, IPool
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int initialSize = 10;
        [SerializeField] private int maxPoolSize = 50;

        private readonly Queue<GameObject> pool = new();
        private readonly HashSet<GameObject> activeObjects = new();

        public void Initialize(GameObject prefab, int size)
        {
            if (prefab == null || prefab.GetComponent<ParticleSystem>() == null) return;

            this.prefab = prefab;
            initialSize = Mathf.Min(size, maxPoolSize);
            
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
            return go;
        }

        public GameObject Get()
        {
            if (pool.Count == 0)
            {
                if (activeObjects.Count < maxPoolSize)
                {
                    var newObj = CreateNew();
                    activeObjects.Add(newObj);
                    return newObj;
                }
                return null;
            }

            var obj = pool.Dequeue();
            activeObjects.Add(obj);
            obj.SetActive(true);
            return obj;
        }

        public void ReturnToPool(GameObject obj)
        {
            if (obj == null) return;

            if (activeObjects.Contains(obj))
            {
                activeObjects.Remove(obj);
            }

            var ps = obj.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            obj.SetActive(false);
            pool.Enqueue(obj);
        }

        public void ReturnToPoolAfterDuration(GameObject obj, float duration)
        {
            if (obj == null) return;
            StartCoroutine(ReturnToPoolAfterDelay(obj, duration));
        }

        private IEnumerator ReturnToPoolAfterDelay(GameObject obj, float duration)
        {
            yield return new WaitForSeconds(duration);
            ReturnToPool(obj);
        }

        private void AddToPool(GameObject obj)
        {
            if (pool.Count < maxPoolSize)
            {
                pool.Enqueue(obj);
            }
            else
            {
                Destroy(obj);
            }
        }
    }
} 