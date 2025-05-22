using Actors;
using Actors.Enum;
using Actors.Factory;
using UnityEngine;

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
            var factory = FindObjectOfType<ElfFactory>();
            if (factory == null) return;
            
            Vector3 offset = new Vector3(
                Random.Range(-1f, 1f),
                0,
                Random.Range(-1f, 1f)
            ).normalized * 5f;
            
            var newElf = factory.CreateEntity(entityType);
            newElf.Initialize(position + offset);
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