using Actors.Factory;
using Actors.Factory.Interface;
using Config;
using Spawner.Interface;
using UnityEngine;
using System.Collections;
using Manager;
using Actors.Enum;
using UnityEngine.Serialization;

namespace Spawner
{
    public class Spawner : MonoBehaviour, ISpawner
    {
        [FormerlySerializedAs("config")] [SerializeField] private SpawnerConfigSO _configSo;
        
        private IEntityFactory entityFactory;
        private Coroutine spawnCoroutine;
        private float currentSpawnTimer;
    
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
            currentSpawnTimer = 0f;
            
            while (true)
            {
                currentSpawnTimer += Time.deltaTime;
                
                if (currentSpawnTimer >= _configSo.spawnInterval)
                {
                    if (EntityCounter.Instance.CanSpawnEntity(_configSo.entityType, _configSo.maxEntities))
                    {
                        SpawnEntity();
                    }
                    currentSpawnTimer = 0f;
                }
                
                yield return null;
            }
        }
        
        private void SpawnEntity()
        {
            var entity = entityFactory?.CreateEntity(_configSo.entityType);
            if (entity == null)
            {
                return;
            }
            
            var spawnPosition = GetValidSpawnPosition(transform.position, _configSo.spawnAreaSize.x);
            if (!spawnPosition.HasValue) return;
            
            entity.Initialize(spawnPosition.Value);
            EntityCounter.Instance.OnEntitySpawned(_configSo.entityType);
            OnEntitySpawned?.Invoke(_configSo.entityType);
        }

        public void SetSpawnInterval(float interval)
        {
            _configSo.spawnInterval = interval;
            currentSpawnTimer = 0f;
            StartSpawning();
        }

        void ISpawner.SpawnEntity()
        {
            SpawnEntity();
        }
        
        public EntityType GetEntityType()
        {
            return _configSo.entityType;
        }

        public SpawnerConfigSO GetConfig()
        {
            return _configSo;
        }
    }
}