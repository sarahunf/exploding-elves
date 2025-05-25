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

namespace Actors
{
    public class Elf : Entity, IPoolable
    {
        [SerializeField] private ElfConfig config;
        [SerializeField] private SpiderElfView view;
        private ParticlePool explosionPool;
        private ParticlePool spawningPool;
        
        private IPool pool;
        private bool isExploding = false;
        private bool canReplicate = true;
        private static HashSet<(int, int)> processedCollisions = new HashSet<(int, int)>();
        private static float lastCleanupTime = 0f;
        private Vector3 lastPosition;
        
        public static event Action<EntityType, Vector3> OnElfReplication;

        private void Awake()
        {
            entityType = config.type;
        }

        public void InitializePools(ParticlePool explosionPool, ParticlePool spawningPool)
        {
            this.explosionPool = explosionPool;
            this.spawningPool = spawningPool;
        }

        private void Update()
        {
            Move();
            bool isMoving = (transform.position - lastPosition).sqrMagnitude > 0.0001f;
            view.SetWalking(isMoving);
            lastPosition = transform.position;
            if (Time.time - lastCleanupTime > config.collisionCleanupInterval)
            {
                processedCollisions.Clear();
                lastCleanupTime = Time.time;
            }
        }
        
        private void OnEnable()
        {
            isExploding = false;
            canReplicate = true;
            view.SetColor(config.color);
            lastPosition = transform.position;
            
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out var hit, 1f))
            {
                Vector3 newPos = new Vector3(transform.position.x, hit.point.y + 0.1f, transform.position.z);
                transform.position = newPos;
            }
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
            yield return new WaitForSeconds(config.replicationCooldown);
            canReplicate = true;
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
                else
                {
                    Debug.LogError($"[ELF_DEBUG] [{gameObject.name}] No ParticleSystem component found on explosion prefab");
                }
            }
            
            if (view != null) {
                Debug.Log($"[ELF_DEBUG] [{gameObject.name}] | animation");
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
            
            Manager.EntityCounter.Instance.OnEntityDestroyed(entityType);
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
            float yDirection = currentDirection.y;
            currentDirection = Vector3.Reflect(currentDirection, collisionNormal);
            currentDirection.y = yDirection;
            transform.position += currentDirection * 0.1f;
            nextDirectionChangeTime = Time.time + config.randomDirectionChangeInterval;
        }
        
        public void SetPool(IPool pool)
        {
            this.pool = pool;
        }
        
        public void ReturnToPool()
        {
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
            Vector3 horizontalMovement = new Vector3(currentDirection.x, 0, currentDirection.z) * (config.moveSpeed * Time.deltaTime);
            Vector3 newPosition = transform.position + horizontalMovement;
            
            if (horizontalMovement != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(horizontalMovement);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
            if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out var hit, 1f))
            {
                float targetHeight = hit.point.y + 0.1f;
                newPosition.y = Mathf.Abs(targetHeight - transform.position.y) <= 0.5f ? targetHeight : transform.position.y;
            }
            else
            {
                newPosition.y = transform.position.y;
            }
            
            transform.position = newPosition;
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
    }
}