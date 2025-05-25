using Actors.Factory;
using Actors.Factory.Interface;
using Config;
using Spawner.Interface;
using UnityEngine;
using System.Collections;
using Manager;
using Actors.Enum;

namespace Spawner
{
    public class Spawner : MonoBehaviour, ISpawner
    {
        [SerializeField] private SpawnerConfig config;
        
        private IEntityFactory entityFactory;
        private Coroutine spawnCoroutine;
    
        private void Awake()
        {
            entityFactory = FindObjectOfType<ElfFactory>();
            if (entityFactory == null)
            {
                Debug.LogError("ElfFactory not found in the scene!");
                return;
            }
        }
    
        private void Start()
        {
            StartSpawning();
        }
    
        private void StartSpawning()
        {
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
            }
        
            spawnCoroutine = StartCoroutine(SpawnRoutine());
        }
    
        private IEnumerator SpawnRoutine()
        {
            while (true)
            {
                if (EntityCounter.Instance.CanSpawnEntity(config.entityType, config.maxEntities))
                {
                    SpawnEntity();
                }
                yield return new WaitForSeconds(config.spawnInterval);
            }
        }
        
        private void SpawnEntity()
        {
            var entity = entityFactory?.CreateEntity(config.entityType);
            if (entity == null)
            {
                Debug.LogError("[Spawner] Failed to create entity - entityFactory returned null");
                return;
            }
            
            int maxAttempts = 10; // Prevent infinite loop
            int attempts = 0;
            
            do
            {
                var randomPosition = transform.position + new Vector3(
                    Random.Range(-config.spawnAreaSize.x/2, config.spawnAreaSize.x/2),
                    0,
                    Random.Range(-config.spawnAreaSize.z/2, config.spawnAreaSize.z/2)
                );
                
                RaycastHit hit;
                Vector3 rayStart = randomPosition + Vector3.up * 100f;
                Vector3 rayEnd = rayStart + Vector3.down * 200f;
                
                Debug.DrawRay(rayStart, Vector3.down * 200f, Color.red, 1f);
                
                if (Physics.Raycast(rayStart, Vector3.down, out hit, 200f))
                {
                    randomPosition.y = hit.point.y + 0.05f;

                    float checkRadius = 0.5f;
                    Collider[] colliders = Physics.OverlapSphere(randomPosition, checkRadius);
                    bool rockNearby = false;
                    foreach (var col in colliders)
                    {
                        if (col.CompareTag("Rock"))
                        {
                            rockNearby = true;
                            break;
                        }
                    }
                    if (rockNearby)
                    {
                        attempts++;
                        continue;
                    }

                    entity.Initialize(randomPosition);
                    EntityCounter.Instance.OnEntitySpawned(config.entityType);
                    return;
                }
                
                attempts++;
            } while (attempts < maxAttempts);
            
            if (attempts >= maxAttempts)
            {
                Debug.LogWarning($"[Spawner] Could not find valid spawn position within ground collider after {maxAttempts} attempts");
                return;
            }
            
            if (entity is MonoBehaviour entityMono)
            {
                Destroy(entityMono.gameObject);
            }
        }

        public void SetSpawnInterval(float interval)
        {
            config.spawnInterval = interval;
            StartSpawning();
        }

        void ISpawner.SpawnEntity()
        {
            SpawnEntity();
        }

        public void OnEntityDestroyed()
        {
            EntityCounter.Instance.OnEntityDestroyed(config.entityType);
        }

        public EntityType GetEntityType()
        {
            return config.entityType;
        }

        public SpawnerConfig GetConfig()
        {
            return config;
        }
    }
}