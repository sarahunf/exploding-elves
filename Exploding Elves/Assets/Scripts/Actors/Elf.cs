using System.Collections;
using Actors.Enum;
using Actors.Interface;
using UnityEngine;
using System;
using System.Collections.Generic;
using IPool = Actors.Pool.IPool;
using IPoolable = Actors.Pool.IPoolable;
using Actors.Pool;
using Config;
using UnityEngine.Serialization;

namespace Actors
{
    public class Elf : Entity, IPoolable
    {
        [FormerlySerializedAs("config")] [SerializeField] private ElfConfigSO _configSo;
        [SerializeField] private SpiderElfView view;
        private ParticlePool explosionPool;
        private ParticlePool spawningPool;
        private MovementComponent movementComponent;
        
        private IPool pool;
        private bool isExploding;
        private bool canReplicate;
        private static HashSet<(int, int)> processedCollisions = new HashSet<(int, int)>();
        private static float lastCleanupTime = 0f;
        
        public static event Action<EntityType, Vector3> OnElfReplication;
        public static event Action<EntityType> OnEntityDestroyed;

        private void Awake()
        {
            entityType = _configSo.type;
            movementComponent = GetComponent<MovementComponent>();
        }

        public void InitializePools(ParticlePool explosionPool, ParticlePool spawningPool)
        {
            this.explosionPool = explosionPool;
            this.spawningPool = spawningPool;
        }

        private void Update()
        {
            if (Time.time - lastCleanupTime > _configSo.collisionCleanupInterval)
            {
                processedCollisions.Clear();
                lastCleanupTime = Time.time;
            }
        }
        
        private void OnEnable()
        {
            isExploding = false;
            canReplicate = false;  // Start with replication disabled
            view.SetBodyColor(_configSo.body);
            view.SetHighlightColor(_configSo.highlight);
            view.SetEmission(true, _configSo.body * 2f); // Enable emission with brighter version of body color
            view.SetScale(false); // Start with smaller scale
            
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out var hit, 1f))
            {
                Vector3 newPos = new Vector3(transform.position.x, hit.point.y + 0.1f, transform.position.z);
                transform.position = newPos;
            }
            StartCoroutine(SpawnCooldown());
        }
        
        private IEnumerator SpawnCooldown()
        {
            yield return new WaitForSeconds(2f);
            canReplicate = true;
            view.SetEmission(false, Color.black);
            view.SetScale(true); // Scale up when can replicate
        }
        
        public override void OnCollision(IEntity other)
        {
            if (isExploding || !canReplicate)
            { 
                return;
            }

            var otherElf = other as Elf;
            if (otherElf == null)
            { 
                return;
            }
            
            int id1 = gameObject.GetInstanceID();
            int id2 = otherElf.gameObject.GetInstanceID();
            var collisionId = (Mathf.Min(id1, id2), Mathf.Max(id1, id2));
            
            if (!processedCollisions.Add(collisionId))
            {
                return;
            }
            
            if (other.GetEntityType() == entityType)
            {
                OnElfReplication?.Invoke(entityType, transform.position);
                
                StartCoroutine(ReplicationCooldown());
                otherElf.StartCoroutine(otherElf.ReplicationCooldown());
            }
            else
            {
                Explode();
                otherElf.Explode();
            }
            StartCoroutine(RemoveCollisionId(collisionId));
        }

        private IEnumerator ReplicationCooldown()
        {
            canReplicate = false;
            view.SetEmission(true, _configSo.body * 2f);
            view.SetScale(false); // Scale down when can't replicate
            yield return new WaitForSeconds(_configSo.replicationCooldown);
            canReplicate = true;
            view.SetEmission(false, Color.black);
            view.SetScale(true); // Scale up when can replicate again
        }

        private IEnumerator RemoveCollisionId((int, int) collisionId)
        {
            yield return new WaitForSeconds(0.1f);
            if (processedCollisions.Contains(collisionId))
            {
                processedCollisions.Remove(collisionId);
            }
        }

        private void Explode()
        {
            if (isExploding) return;
            
            isExploding = true;

            if (explosionPool != null)
            {
                var explosion = explosionPool.Get();
                if (explosion == null)
                {
                    return;
                }
                
                explosion.transform.position = transform.position;
                var particleSystem = explosion.GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    particleSystem.Play();
                    explosionPool.ReturnToPoolAfterDuration(explosion, (particleSystem.main.duration + particleSystem.main.startDelay.constant) + 0.2f);
                }
            }
            
            if (view != null) {
                view.AttackAndDestroy(OnAttackFinished);
            } else {
                StartCoroutine(DelayedReturn());
            }
        }
        private void OnAttackFinished()
        {
            StartCoroutine(DelayedReturn());
        }

        private IEnumerator DelayedReturn()
        {
            yield return new WaitForSeconds(0.1f);
            
            if (isExploding)  // Prevent multiple returns
            {
                Manager.EntityCounter.Instance.OnEntityDestroyed(entityType);
                OnEntityDestroyed?.Invoke(entityType);
                
                // Reset state before returning to pool
                isExploding = false;
                canReplicate = true;
                
                ReturnToPool();
            }
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
                OnCollision(otherElf);
            }
            else if (other.CompareTag($"Rock"))
            {
                HandleRockCollision(other);
            }
        }

        private void HandleRockCollision(Collider rock)
        {
            Vector3 collisionNormal = (transform.position - rock.transform.position).normalized;
            Vector3 currentDirection = movementComponent.GetCurrentDirection();
            float yDirection = currentDirection.y;
            Vector3 newDirection = Vector3.Reflect(currentDirection, collisionNormal);
            newDirection.y = yDirection;
            movementComponent.SetDirection(newDirection);
            movementComponent.SetNextDirectionChangeTime(Time.time + _configSo.randomDirectionChangeInterval);
            transform.position += newDirection * 0.1f;
        }
        
        public void SetPool(IPool pool)
        {
            this.pool = pool;
        }
        
        public void ReturnToPool()
        {
            if (pool != null)
            {
                pool.ReturnToPool(gameObject);
            }
        }
        
        public void ShowSpawnEffect(Vector3 position)
        {
            if (spawningPool == null) return;
            
            var spawnEffect = spawningPool.Get();
            if (spawnEffect == null) return;
                
            spawnEffect.transform.position = position;
            var particleSystem = spawnEffect.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                particleSystem.Play();
                spawningPool.ReturnToPoolAfterDuration(spawnEffect, (particleSystem.main.duration + particleSystem.main.startDelay.constant) + 0.2f);
            }
        }
        
        public ElfConfigSO GetConfig() => _configSo;
        public SpiderElfView GetView() => view;
    }
}