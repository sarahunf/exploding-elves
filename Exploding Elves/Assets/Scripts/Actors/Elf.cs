using System.Collections;
using Actors.Enum;
using Actors.Interface;
using Config;
using UnityEngine;
using System;
using System.Collections.Generic;
using IPool = Actors.Pool.IPool;
using IPoolable = Actors.Pool.IPoolable;

namespace Actors
{
    public class Elf : Entity, IPoolable
    {
        [SerializeField] private ElfConfig config;
        
        private IPool pool;
        private bool isExploding = false;
        private bool canReplicate = true;
        private static HashSet<(int, int)> processedCollisions = new HashSet<(int, int)>();
        private static float lastCleanupTime = 0f;
        
        public static event Action<EntityType, Vector3> OnElfReplication;

        private void Awake()
        {
            entityType = config.type;
            Debug.Log($"[{gameObject.name}] Awake - Type: {config.type}");
        }

        private void Update()
        {
            Move();
            if (Time.time - lastCleanupTime > config.collisionCleanupInterval)
            {
                processedCollisions.Clear();
                lastCleanupTime = Time.time;
                Debug.Log($"[{gameObject.name}] Cleaned up processed collisions HashSet");
            }
        }
        
        private void OnEnable()
        {
            isExploding = false;
            canReplicate = true;
            Debug.Log($"[{gameObject.name}] Enabled - Type: {config.type}");
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
            yield return new WaitForSeconds(config.replicationCooldown);
            canReplicate = true;
        }

        private IEnumerator RemoveCollisionId((int, int) collisionId)
        {
            yield return new WaitForSeconds(0.1f);
            if (processedCollisions.Contains(collisionId))
            {
                processedCollisions.Remove(collisionId);
                Debug.Log($"[{gameObject.name}] Removed collision ID from processed collisions");
            }
        }

        private void Explode()
        {
            if (isExploding) return;
            
            isExploding = true;
            Debug.Log($"[{gameObject.name}] Starting explosion");

            if (config.explosionEffect != null)
            {
                var explosion = Instantiate(config.explosionEffect, transform.position, Quaternion.identity);
                explosion.Play();
                Destroy(explosion.gameObject, explosion.main.duration);
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
            if (otherElf == null)
            {
                otherElf = other.GetComponentInParent<Elf>();
            }
            
            if (otherElf != null)
            {
                Debug.Log($"[{gameObject.name}] Trigger entered with {otherElf.gameObject.name}");
                OnCollision(otherElf);
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] Trigger entered with object that has no Elf component: {other.gameObject.name}");
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
        
        protected override void Move()
        {
            if (currentDirection == Vector3.zero || Time.time >= nextDirectionChangeTime)
            {
                currentDirection = new Vector3(
                    UnityEngine.Random.Range(-1f, 1f),
                    0,
                    UnityEngine.Random.Range(-1f, 1f)
                ).normalized;
                
                nextDirectionChangeTime = Time.time + config.randomDirectionChangeInterval;
            }
            
            transform.position += currentDirection * (config.moveSpeed * Time.deltaTime);
        }
    }
}