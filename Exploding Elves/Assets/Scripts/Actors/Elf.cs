using System;
using Actors.Enum;
using Actors.Interface;
using Actors.StateMachine;
using Actors.Pool;
using Actors.Components;
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
        
        private MovementComponent movementComponent;
        private ElfStateMachine stateMachine;
        private CollisionHandler collisionHandler;
        private ParticleEffectHandler particleHandler;
        private IPool pool;
        
        public static event Action<EntityType, Vector3> OnElfReplication;
        public static event Action<EntityType> OnEntityDestroyed;

        private void Awake()
        {
            entityType = _configSo.type;
            movementComponent = GetComponent<MovementComponent>();
            collisionHandler = GetComponent<CollisionHandler>();
            particleHandler = GetComponent<ParticleEffectHandler>();
            stateMachine = new ElfStateMachine(this);
        }

        public void InitializePools(ParticlePool explosionPool, ParticlePool spawningPool)
        {
            particleHandler.InitializePools(explosionPool, spawningPool);
        }

        private void Update()
        {
            stateMachine.Update();
        }
        
        private void OnEnable()
        {
            stateMachine.SetState(ElfState.Spawning, 2f);
            view.SetBodyColor(_configSo.body);
            view.SetHighlightColor(_configSo.highlight);
            
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out var hit, 1f))
            {
                var newPos = new Vector3(transform.position.x, hit.point.y + 0.1f, transform.position.z);
                transform.position = newPos;
            }
        }
        
        public override void OnCollision(IEntity other)
        {
            base.OnCollision(other);
            collisionHandler.HandleCollision(other);
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
                        collisionHandler.HandleRockCollision(other);
                    }
                    return;
                }
            }
            
            OnCollision(otherElf);
        }

        public void HandleReplication()
        {
            OnElfReplication?.Invoke(entityType, transform.position);
            stateMachine.SetState(ElfState.Replicating, _configSo.replicationCooldown);
        }

        public void Explode()
        {
            if (stateMachine.GetCurrentState() == ElfState.Exploding) return;
            
            stateMachine.SetState(ElfState.Exploding);
            particleHandler.ShowExplosionEffect();
            StartCoroutine(DelayedReturn());
        }

        private System.Collections.IEnumerator DelayedReturn()
        {
            yield return null;
            if (stateMachine.GetCurrentState() != ElfState.Exploding) yield break;
            
            Manager.EntityCounter.Instance.OnEntityDestroyed(entityType);
            OnEntityDestroyed?.Invoke(entityType);
                
            stateMachine.SetState(ElfState.Spawning, 2f);
            ReturnToPool();
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
            particleHandler.ShowSpawnEffect(position);
        }
        
        public ElfConfigSO GetConfig() => _configSo;
        public SpiderElfView GetView() => view;
        public bool CanReplicate() => stateMachine.CanReplicate();
    }
}