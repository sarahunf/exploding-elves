using System;
using System.Collections;
using System.Collections.Generic;
using Actors.Enum;
using Actors.Interface;
using Actors.StateMachine;
using Actors.Pool;
using Config;
using UnityEngine;
using UnityEngine.Serialization;
using IPool = Actors.Pool.IPool;
using IPoolable = Actors.Pool.IPoolable;


namespace Actors
{
    public class Elf : Entity, IPoolable
    {
        [FormerlySerializedAs("config")] [SerializeField] private ElfConfigSO _configSo;
        [SerializeField] private SpiderElfView view;
        private ParticlePool explosionPool;
        private ParticlePool spawningPool;
        private MovementComponent movementComponent;
        private ElfStateMachine stateMachine;
        
        private IPool pool;
        private static HashSet<(int, int)> processedCollisions = new HashSet<(int, int)>();
        private static float lastCleanupTime = 0f;
        
        public static event Action<EntityType, Vector3> OnElfReplication;
        public static event Action<EntityType> OnEntityDestroyed;

        private void Awake()
        {
            entityType = _configSo.type;
            movementComponent = GetComponent<MovementComponent>();
            stateMachine = new ElfStateMachine(this);
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

            stateMachine.Update();
        }
        
        private void OnEnable()
        {
            stateMachine.SetState(ElfState.Spawning, 2f);
            view.SetBodyColor(_configSo.body);
            view.SetHighlightColor(_configSo.highlight);
            
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out var hit, 1f))
            {
                Vector3 newPos = new Vector3(transform.position.x, hit.point.y + 0.1f, transform.position.z);
                transform.position = newPos;
            }
        }
        
        public override void OnCollision(IEntity other)
        {
            if (!stateMachine.CanReplicate())
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
                
                stateMachine.SetState(ElfState.Replicating, _configSo.replicationCooldown);
                otherElf.stateMachine.SetState(ElfState.Replicating, _configSo.replicationCooldown);
            }
            else
            {
                Explode();
                otherElf.Explode();
            }
            StartCoroutine(RemoveCollisionId(collisionId));
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
            if (stateMachine.GetCurrentState() == ElfState.Exploding) return;
            
            stateMachine.SetState(ElfState.Exploding);

            if (explosionPool != null)
            {
                var explosion = explosionPool.Get();
                if (explosion != null)
                {
                    explosion.transform.position = transform.position;
                    var particleSystem = explosion.GetComponent<ParticleSystem>();
                    if (particleSystem != null)
                    {
                        var main = particleSystem.main;
                        particleSystem.Play();
                        explosionPool.ReturnToPoolAfterDuration(explosion, (main.duration + main.startDelay.constant) + 0.2f);
                    }
                }
            }
            
            StartCoroutine(DelayedReturn());
        }

        private IEnumerator DelayedReturn()
        {
            yield return null;
            
            if (stateMachine.GetCurrentState() == ElfState.Exploding)
            {
                Manager.EntityCounter.Instance.OnEntityDestroyed(entityType);
                OnEntityDestroyed?.Invoke(entityType);
                
                stateMachine.SetState(ElfState.Spawning, 2f);
                ReturnToPool();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!stateMachine.CanReplicate()) return;

            var otherElf = other.GetComponent<Elf>();
            if (otherElf == null)
            {
                otherElf = other.GetComponentInParent<Elf>();
                if (otherElf == null)
                {
                    if (other.CompareTag("Rock"))
                    {
                        HandleRockCollision(other);
                    }
                    return;
                }
            }
            
            OnCollision(otherElf);
        }

        private void HandleRockCollision(Collider rock)
        {
            if (movementComponent == null) return;

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
                var main = particleSystem.main;
                particleSystem.Play();
                spawningPool.ReturnToPoolAfterDuration(spawnEffect, (main.duration + main.startDelay.constant) + 0.2f);
            }
        }
        
        public ElfConfigSO GetConfig() => _configSo;
        public SpiderElfView GetView() => view;
    }
}