using Actors;
using Actors.Enum;
using Actors.Factory;
using Config;
using UnityEngine;

namespace Manager
{
    public class ElfReplicationManager : MonoBehaviour
    {
        [SerializeField] private Spawner.Spawner[] spawners;
        [SerializeField] private ElfFactory factory;

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
            if (!EntityCounter.Instance.CanSpawnEntity(entityType, configSo.maxEntities)) return;
            if (factory == null) return;
            
            var newElf = factory.CreateEntity(entityType);
            if (newElf == null) return;

            var spawnPosition = Spawner.Spawner.GetValidSpawnPosition(position, 5f);
            if (spawnPosition.HasValue)
            {
                newElf.Initialize(spawnPosition.Value);
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
    }
} 