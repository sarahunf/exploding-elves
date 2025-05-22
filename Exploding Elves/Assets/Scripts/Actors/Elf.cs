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
        [SerializeField] private ParticlePool explosionPool;
        
        private IPool pool;
        private bool isExploding = false;
        private bool canReplicate = true;
        private static HashSet<(int, int)> processedCollisions = new();
        
        public static event Action<EntityType, Vector3> OnElfReplication;

        private void Awake()
        {
            entityType = type;
            Debug.Log($"[{gameObject.name}] Awake - Type: {type}");
            explosionPool = FindObjectOfType<ParticlePool>();
        }

        private void Update()
        {
            Move();
        }
        
        private void OnEnable()
        {
            isExploding = false;
            canReplicate = true;
            Debug.Log($"[{gameObject.name}] Enabled - Type: {type}");
        }
        
        public override void OnCollision(IEntity other)
        {
            if (isExploding || !canReplicate) return;

            var otherElf = other as Elf;
            if (otherElf == null) return;
            
            int id1 = gameObject.GetInstanceID();
            int id2 = otherElf.gameObject.GetInstanceID();
            var collisionId = (Mathf.Min(id1, id2), Mathf.Max(id1, id2));
            
            if (!processedCollisions.Add(collisionId)) return;

            Debug.Log($"[{gameObject.name}] Processing collision with {other.GetEntityType()}");
            
            if (other.GetEntityType() == entityType)
            {
                Debug.Log($"[{gameObject.name}] Same type collision - triggering replication");
                OnElfReplication?.Invoke(entityType, transform.position);
                
                StartCoroutine(ReplicationCooldown());
                otherElf.StartCoroutine(otherElf.ReplicationCooldown());
            }
            else
            {
                Debug.Log($"[{gameObject.name}] Different type collision - exploding");
                Explode();
                otherElf.Explode();
            }
            StartCoroutine(RemoveCollisionId(collisionId));
        }

        private IEnumerator ReplicationCooldown()
        {
            canReplicate = false;
            yield return new WaitForSeconds(20f);
            canReplicate = true;
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
                if (explosion != null)
                {
                    var ps = explosion.GetComponent<ParticleSystem>();
                    if (ps != null)
                    {
                        explosion.transform.position = transform.position;
                        ps.Play();
                        explosionPool.ReturnToPoolAfterDuration(explosion, ps.main.duration);
                    }
                    else
                    {
                        Debug.LogError($"[{gameObject.name}] Got non-particle system object from explosion pool! Object type: {explosion.GetType()}");
                        explosionPool.ReturnToPool(explosion);
                    }
                }
            }
            
            StartCoroutine(DelayedReturn());
        }

        public IEnumerator DelayedReturn()
        {
            Debug.Log($"[{gameObject.name}] Delayed return to pool");
            var component = GetComponent<Collider>();
            if (component != null) component.enabled = false;
            
            yield return new WaitForSeconds(0.1f);
            
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
        

        public void SetPool(IPool pool)
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