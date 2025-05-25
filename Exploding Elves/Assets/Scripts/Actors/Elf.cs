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
        [SerializeField] private SpiderElfView view;
        
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
            else
            {
                Debug.LogWarning($"[ELF_DEBUG] [{gameObject.name}] Spawn - No ground detected at initial position {transform.position}");
            }
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

            if (config.explosionEffect != null)
            {
                var explosion = Instantiate(config.explosionEffect, transform.position, Quaternion.identity);
                explosion.Play();
                Destroy(explosion.gameObject, explosion.main.duration);
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

        public IEnumerator DelayedReturn()
        {
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
                OnCollision(otherElf);
            }
            else if (other.CompareTag($"Rock"))
            {
                HandleRockCollision(other);
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] Trigger entered with object that has no Elf component: {other.gameObject.name}");
                return;
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
            var component = GetComponent<Collider>();
            if (component != null) component.enabled = true;
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
            
            // Calculate horizontal movement
            Vector3 horizontalMovement = new Vector3(currentDirection.x, 0, currentDirection.z) * (config.moveSpeed * Time.deltaTime);
            Vector3 newPosition = transform.position + horizontalMovement;
            
            // Check for ground directly below
            if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out var hit, 1f))
            {
                float targetHeight = hit.point.y + 0.1f;
                // Only allow height changes within a reasonable range (0.5 units)
                if (Mathf.Abs(targetHeight - transform.position.y) <= 0.5f)
                {
                    newPosition.y = targetHeight;
                }
                else
                {
                    // If height difference is too large, maintain current height
                    newPosition.y = transform.position.y;
                }
            }
            else
            {
                // If no ground found, maintain current height
                newPosition.y = transform.position.y;
            }
            
            transform.position = newPosition;
        }
    }
}