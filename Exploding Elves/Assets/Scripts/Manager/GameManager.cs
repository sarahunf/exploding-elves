using Actors;
using Actors.Enum;
using Actors.Factory;
using UnityEngine;
using Config;

namespace Manager
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Spawner.Spawner[] spawners;
    
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
            newElf.Initialize(position + offset);
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