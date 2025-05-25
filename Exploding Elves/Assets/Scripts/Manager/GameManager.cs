using Actors;
using Actors.Enum;
using Actors.Factory;
using Actors.Pool;
using UnityEngine;
using Config;

namespace Manager
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Spawner.Spawner[] spawners;
        [SerializeField] private ParticlePool explosionPool;
        [SerializeField] private ParticlePool spawningPool;

        private static GameManager instance;
        public static GameManager Instance => instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public ParticlePool GetExplosionPool() => explosionPool;
        public ParticlePool GetSpawningPool() => spawningPool;
    
        private void OnEnable()
        {
            Elf.OnElfReplication += HandleElfReplication;
        }
    
        private void OnDisable()
        {
            Elf.OnElfReplication -= HandleElfReplication;
        }
    
        private void HandleElfReplication(EntityType entityType, Vector3 position)
        {
            // Find the spawner config for this entity type
            SpawnerConfig config = null;
            foreach (var spawner in spawners)
            {
                if (spawner.GetEntityType() == entityType)
                {
                    config = spawner.GetConfig();
                    break;
                }
            }
            
            if (config == null) return;
            
            // Check if we can spawn more entities of this type
            if (!EntityCounter.Instance.CanSpawnEntity(entityType, config.maxEntities))
            {
                return;
            }
            
            var factory = FindObjectOfType<ElfFactory>();
            if (factory == null) return;
            
            Vector3 offset = new Vector3(
                Random.Range(-1f, 1f),
                0,
                Random.Range(-1f, 1f)
            ).normalized * 5f;
            
            var newElf = factory.CreateEntity(entityType);
            Vector3 spawnPosition = position + offset;
            newElf.Initialize(spawnPosition);
            
            // Show spawn effect
            if (newElf is Elf elf)
            {
                elf.ShowSpawnEffect(spawnPosition);
            }
            
            EntityCounter.Instance.OnEntitySpawned(entityType);
        }
    
        public void SetSpawnerInterval(int spawnerIndex, float interval)
        {
            if (spawnerIndex >= 0 && spawnerIndex < spawners.Length)
            {
                spawners[spawnerIndex].SetSpawnInterval(interval);
            }
        }
    }
}