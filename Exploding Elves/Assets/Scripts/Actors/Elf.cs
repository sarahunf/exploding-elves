using System.Collections;
using Actors.Enum;
using Actors.Interface;
using Config;
using UnityEngine;
using System;
using System.Collections.Generic;
using IPool = Actors.Pool.IPool;
using IPoolable = Actors.Pool.IPoolable;
using Actors.Pool;

namespace Actors
{
    public class Elf : Entity, IPoolable
    {
        [SerializeField] private ElfConfig config;
        [SerializeField] private SpiderElfView view;
        private ParticlePool explosionPool;
        
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

        private void Start()
        {
            //TODO: inject pool instead of find object
            explosionPool = FindObjectOfType<ParticlePool>();
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
            Debug.Log($"[ELF_DEBUG] [{gameObject.name}] OnCollision called with: {other.GetType().Name}");
            
            if (isExploding || !canReplicate)
            {
                Debug.Log($"[ELF_DEBUG] [{gameObject.name}] Collision ignored - isExploding: {isExploding}, canReplicate: {canReplicate}");
                return;
            }

            var otherElf = other as Elf;
            if (otherElf == null)
            {
                Debug.LogWarning($"[ELF_DEBUG] [{gameObject.name}] Other entity is not an Elf");
                return;
            }
            
            int id1 = gameObject.GetInstanceID();
            int id2 = otherElf.gameObject.GetInstanceID();
            var collisionId = (Mathf.Min(id1, id2), Mathf.Max(id1, id2));
            
            if (!processedCollisions.Add(collisionId))
            {
                Debug.Log($"[ELF_DEBUG] [{gameObject.name}] Collision already processed");
                return;
            }

            Debug.Log($"[ELF_DEBUG] [{gameObject.name}] Processing collision with {otherElf.gameObject.name}");
            
            if (other.GetEntityType() == entityType)
            {
                Debug.Log($"[ELF_DEBUG] [{gameObject.name}] Same type collision - triggering replication");
                OnElfReplication?.Invoke(entityType, transform.position);
                
                StartCoroutine(ReplicationCooldown());
                otherElf.StartCoroutine(otherElf.ReplicationCooldown());
            }
            else
            {
                Debug.Log($"[ELF_DEBUG] [{gameObject.name}] Different type collision - triggering explosion");
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
            Debug.Log($"[ELF_DEBUG] [{gameObject.name}] Starting explosion");

            if (explosionPool != null)
            {
                Debug.Log($"[ELF_DEBUG] [{gameObject.name}] Getting explosion from pool");
                var explosion = explosionPool.Get();
                if (explosion == null)
                {
                    Debug.LogError($"[ELF_DEBUG] [{gameObject.name}] Failed to get explosion from pool");
                    return;
                }
                
                explosion.transform.position = transform.position;
                var particleSystem = explosion.GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    Debug.Log($"[ELF_DEBUG] [{gameObject.name}] Playing particle system");
                    particleSystem.Play();
                    explosionPool.ReturnToPoolAfterDuration(explosion, (particleSystem.main.duration + particleSystem.main.startDelay.constant) + 0.2f);
                }
                else
                {
                    Debug.LogError($"[ELF_DEBUG] [{gameObject.name}] No ParticleSystem component found on explosion prefab");
                }
            }
            else
            {
                Debug.LogWarning($"[ELF_DEBUG] [{gameObject.name}] Missing explosion setup - config.explosionEffect: {config.explosionEffect != null}, explosionPool: {explosionPool != null}");
            }
            
            if (view != null) {
                Debug.Log($"[ELF_DEBUG] [{gameObject.name}] Triggering view attack animation");
                view.AttackAndDestroy(OnAttackFinished);
            } else {
                Debug.LogWarning($"[ELF_DEBUG] [{gameObject.name}] No view component found, using delayed return");
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
            Debug.Log($"[ELF_DEBUG] [{gameObject.name}] Trigger entered with: {other.gameObject.name}");
            
            var otherElf = other.GetComponent<Elf>();
            if (otherElf == null)
            {
                otherElf = other.GetComponentInParent<Elf>();
            }
            
            if (otherElf != null)
            {
                Debug.Log($"[ELF_DEBUG] [{gameObject.name}] Found other elf: {otherElf.gameObject.name}");
                OnCollision(otherElf);
            }
            else if (other.CompareTag($"Rock"))
            {
                Debug.Log($"[ELF_DEBUG] [{gameObject.name}] Collided with rock");
                HandleRockCollision(other);
            }
            else
            {
                Debug.LogWarning($"[ELF_DEBUG] [{gameObject.name}] Trigger entered with object that has no Elf component: {other.gameObject.name}");
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
            
            // Rotate to face movement direction
            if (horizontalMovement != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(horizontalMovement);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
            
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