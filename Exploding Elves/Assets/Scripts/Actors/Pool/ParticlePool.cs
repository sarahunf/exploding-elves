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
                return;
            }

            if (prefab.GetComponent<ParticleSystem>() == null)
            {
                return;
            }

            this.prefab = prefab;
            initialSize = size;
            
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
                return CreateNew();
            }

            var obj = pool.Dequeue();
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