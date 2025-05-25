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
            SpawnerConfigSO configSo = null;
            foreach (var spawner in spawners)
            {
                if (spawner.GetEntityType() == entityType)
                {
                    configSo = spawner.GetConfig();
                    break;
                }
            }
            
            if (configSo == null) return;
            
            // Check if we can spawn more entities of this type
            if (!EntityCounter.Instance.CanSpawnEntity(entityType, configSo.maxEntities))
            {
                return;
            }
            
            var factory = FindObjectOfType<ElfFactory>();
            if (factory == null) return;
            
            var newElf = factory.CreateEntity(entityType);
            if (newElf == null) return;

            var spawnPosition = Spawner.Spawner.GetValidSpawnPosition(position, 5f);
            if (spawnPosition.HasValue)
            {
                newElf.Initialize(spawnPosition.Value);
                
                // Show spawn effect
                if (newElf is Elf elf)
                {
                    elf.ShowSpawnEffect(spawnPosition.Value);
                }
                
                EntityCounter.Instance.OnEntitySpawned(entityType);
            }
            else
            {
                if (newElf is MonoBehaviour entityMono)
                {
                    Destroy(entityMono.gameObject);
                }
            }
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