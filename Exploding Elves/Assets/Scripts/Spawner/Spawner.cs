using Actors.Enum;
using Actors.Factory;
using Actors.Factory.Interface;
using Actors.Interface;
using Spawner.Interface;

namespace Spawner
{
    using UnityEngine;
    using System.Collections;

    public class Spawner : MonoBehaviour, ISpawner
    {
        [SerializeField] private EntityType entityType;
        [SerializeField] private float spawnInterval = 2f;
        [SerializeField] private Vector3 spawnAreaSize = new Vector3(5f, 0, 5f);
        
        [SerializeField] private int maxEntities = 100;
        private int currentEntities = 0;
    
        private IEntityFactory entityFactory;
        private Coroutine spawnCoroutine;
    
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
                SpawnEntity();
                yield return new WaitForSeconds(spawnInterval);
            }
        }
    
        public void SpawnEntity()
        {
            if (entityFactory == null) return;
        
            Vector3 randomPosition = transform.position + new Vector3(
                Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
                0,
                Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
            );
        
            IEntity entity = entityFactory.CreateEntity(entityType);
            entity.Initialize(randomPosition);
        }
    
        public void SetSpawnInterval(float interval)
        {
            spawnInterval = Mathf.Max(0.1f, interval);
            StartSpawning();
        }
    
        public EntityType GetEntityType()
        {
            return entityType;
        }
    }
}