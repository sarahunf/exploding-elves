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
    
        public static event System.Action<EntityType> OnEntitySpawned;
    
        public static Vector3? GetValidSpawnPosition(Vector3 basePosition, float areaSize)
        {
            int maxAttempts = 10;
            int attempts = 0;
            
            do
            {
                var randomPosition = basePosition + new Vector3(
                    Random.Range(-areaSize/2, areaSize/2),
                    0,
                    Random.Range(-areaSize/2, areaSize/2)
                );
                
                RaycastHit hit;
                Vector3 rayStart = randomPosition + Vector3.up * 100f;
                
                if (Physics.Raycast(rayStart, Vector3.down, out hit, 200f) && hit.collider.CompareTag("Ground"))
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
                    if (!rockNearby)
                    {
                        return randomPosition;
                    }
                }
                
                attempts++;
            } while (attempts < maxAttempts);
            
            return null;
        }
    
        private void Awake()
        {
            entityFactory = FindObjectOfType<ElfFactory>();
            if (entityFactory == null)
            {
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
                return;
            }
            
            var spawnPosition = GetValidSpawnPosition(transform.position, config.spawnAreaSize.x);
            if (!spawnPosition.HasValue) return;
            
            entity.Initialize(spawnPosition.Value);
            EntityCounter.Instance.OnEntitySpawned(config.entityType);
            OnEntitySpawned?.Invoke(config.entityType);
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