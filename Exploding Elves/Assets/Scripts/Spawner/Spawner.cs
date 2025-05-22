using Actors.Factory;
using Actors.Factory.Interface;
using Config;
using Spawner.Interface;
using UnityEngine;
using System.Collections;

namespace Spawner
{
    public class Spawner : MonoBehaviour, ISpawner
    {
        [SerializeField] private SpawnerConfig config;
        
        private IEntityFactory entityFactory;
        private Coroutine spawnCoroutine;
        private int currentEntities = 0;
    
        private void Awake()
        {
            entityFactory = FindObjectOfType<ElfFactory>();
            if (entityFactory == null)
            {
                Debug.LogError("ElfFactory not found in the scene!");
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
                if (currentEntities < config.maxEntities)
                {
                    SpawnEntity();
                }
                yield return new WaitForSeconds(config.spawnInterval);
            }
        }
        
        private void SpawnEntity()
        {
            if (entityFactory == null) return;
            
            var entity = entityFactory.CreateEntity(config.entityType);
            if (entity == null) return;
            
            Vector3 randomPosition = transform.position + new Vector3(
                Random.Range(-config.spawnAreaSize.x/2, config.spawnAreaSize.x/2),
                0,
                Random.Range(-config.spawnAreaSize.z/2, config.spawnAreaSize.z/2)
            );
            
            entity.Initialize(randomPosition);
            currentEntities++;
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
            currentEntities--;
        }
    }
}