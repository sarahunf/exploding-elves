using System.Collections;
using Actors.Enum;
using Actors.Interface;
using Actors.Pool;

namespace Actors
{
    using UnityEngine;
    using System;
    using System.Collections.Generic;

    public class Elf : Entity, IPoolable
    {
        [SerializeField] private ParticleSystem explosionEffect;
        [SerializeField] private EntityType type;
        
        private EntityPool pool;
        private bool isExploding = false;
        private static EntityPool explosionPool;
        private static HashSet<(int, int)> processedCollisions = new();
        
        public static event Action<EntityType, Vector3> OnElfReplication;

        private void Awake()
        {
            entityType = type;
            Debug.Log($"[{gameObject.name}] Awake - Type: {type}");
            
            if (explosionPool == null && explosionEffect != null)
            {
                var poolObj = new GameObject("ExplosionPool");
                explosionPool = poolObj.AddComponent<EntityPool>();
                explosionPool.Initialize(explosionEffect.gameObject, 10);
            }
        }

        private void Update()
        {
            Move();
        }
        
        private void OnEnable()
        {
            isExploding = false;
            Debug.Log($"[{gameObject.name}] Enabled - Type: {type}");
        }
        
        public override void OnCollision(IEntity other)
        {
            if (isExploding) return;

            var otherElf = other as Elf;
            if (otherElf == null) return;

            // Create a unique collision ID using the instance IDs
            int id1 = gameObject.GetInstanceID();
            int id2 = otherElf.gameObject.GetInstanceID();
            var collisionId = (Mathf.Min(id1, id2), Mathf.Max(id1, id2));

            // Skip if this collision was already processed
            if (processedCollisions.Contains(collisionId)) return;
            processedCollisions.Add(collisionId);

            Debug.Log($"[{gameObject.name}] Processing collision with {other.GetEntityType()}");
            
            if (other.GetEntityType() == entityType)
            {
                Debug.Log($"[{gameObject.name}] Same type collision - triggering replication");
                OnElfReplication?.Invoke(entityType, transform.position);
                // Return both elves to pool after replication
                StartCoroutine(DelayedReturn());
                otherElf.StartCoroutine(otherElf.DelayedReturn());
            }
            else
            {
                Debug.Log($"[{gameObject.name}] Different type collision - exploding");
                Explode();
                otherElf.Explode();
            }
            

            // Remove the collision ID after a short delay
            StartCoroutine(RemoveCollisionId(collisionId));
        }

        private IEnumerator RemoveCollisionId((int, int) collisionId)
        {
            yield return new WaitForSeconds(0.1f);
            processedCollisions.Remove(collisionId);
        }

        private void Explode()
        {
            if (isExploding) return;
            isExploding = true;
            Debug.Log($"[{gameObject.name}] Starting explosion");

            if (explosionEffect != null && explosionPool != null)
            {
                var explosion = explosionPool.Get();
                explosion.transform.position = transform.position;
                var particleSystem = explosion.GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    particleSystem.Play();
                    StartCoroutine(ReturnExplosionToPool(explosion, particleSystem.main.duration));
                }
            }

            StartCoroutine(DelayedReturn());
        }

        private IEnumerator ReturnExplosionToPool(GameObject explosion, float duration)
        {
            yield return new WaitForSeconds(duration);
            explosionPool.ReturnToPool(explosion);
        }

        public IEnumerator DelayedReturn()
        {
            Debug.Log($"[{gameObject.name}] Delayed return to pool");
            var component = GetComponent<Collider>();
            if (component != null) component.enabled = false;
            yield return null;

            ReturnToPool();
        }

        private void OnTriggerEnter(Collider other)
        {
            var otherElf = other.GetComponent<Elf>();
            if (otherElf != null)
            {
                OnCollision(otherElf);
            }
        }

        public void SetPool(EntityPool pool)
        {
            Debug.Log($"[{gameObject.name}] Setting pool reference");
            this.pool = pool;
            var component = GetComponent<Collider>();
            if (component != null) component.enabled = true;
        }

        public void ReturnToPool()
        {
            Debug.Log($"[{gameObject.name}] Returning to pool");
            pool?.ReturnToPool(gameObject);
        }
    }
}