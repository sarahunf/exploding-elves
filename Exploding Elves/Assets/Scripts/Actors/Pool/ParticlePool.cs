using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actors.Pool
{
    public class ParticlePool : MonoBehaviour, IPool
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int initialSize = 10;

        private readonly Queue<GameObject> pool = new();

        public void Initialize(GameObject prefab, int size)
        {
            if (prefab == null)
            {
                Debug.LogError($"[{gameObject.name}] Cannot initialize pool with null prefab!");
                return;
            }

            var particleSystem = prefab.GetComponent<ParticleSystem>();
            if (particleSystem == null)
            {
                Debug.LogError($"[{gameObject.name}] Prefab must have a ParticleSystem component!");
                return;
            }

            this.prefab = prefab;
            initialSize = size;
            
            Debug.Log($"[{gameObject.name}] Initializing particle pool with {initialSize} objects");
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
            Debug.Log($"[{gameObject.name}] Creating new particle effect in pool");
            GameObject go = Instantiate(prefab, transform);
            go.SetActive(false);
            return go;
        }

        public GameObject Get()
        {
            if (pool.Count == 0)
            {
                Debug.LogWarning($"[{gameObject.name}] Particle pool is empty! Creating new object");
                AddToPool(CreateNew());
            }

            var obj = pool.Dequeue();
            Debug.Log($"[{gameObject.name}] Getting particle effect from pool. Remaining: {pool.Count}");
            obj.SetActive(true);
            return obj;
        }

        public void ReturnToPool(GameObject obj)
        {
            if (obj == null) return;

            var ps = obj.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop();
                ps.Clear();
            }

            Debug.Log($"[{gameObject.name}] Returning particle effect to pool. Current size: {pool.Count}");
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
            pool.Enqueue(obj);
        }
    }
} 