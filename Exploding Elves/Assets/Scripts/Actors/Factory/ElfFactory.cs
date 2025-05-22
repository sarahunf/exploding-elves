using System.Collections.Generic;
using Actors.Enum;
using Actors.Factory.Interface;
using Actors.Interface;
using Actors.Pool;
using UnityEngine;

namespace Actors.Factory
{
    public class ElfFactory : MonoBehaviour, IEntityFactory
    {
        [Header("Pools for each elf type")]
        [SerializeField] private EntityPool blackElfPool;
        [SerializeField] private EntityPool redElfPool;
        [SerializeField] private EntityPool whiteElfPool;
        [SerializeField] private EntityPool blueElfPool;

        private Dictionary<EntityType, EntityPool> poolMap;

        private void Awake()
        {
            Debug.Log("[ElfFactory] Initializing pool map");
            poolMap = new Dictionary<EntityType, EntityPool>
            {
                { EntityType.BlackElf, blackElfPool },
                { EntityType.RedElf, redElfPool },
                { EntityType.WhiteElf, whiteElfPool },
                { EntityType.BlueElf, blueElfPool },
            };
        }

        public IEntity CreateEntity(EntityType entityType)
        {
            Debug.Log($"[ElfFactory] Creating entity of type {entityType}");
            if (!poolMap.TryGetValue(entityType, out var pool))
            {
                Debug.LogError($"[ElfFactory] No pool found for {entityType}");
                return null;
            }

            GameObject obj = pool.Get();
            var entity = obj.GetComponent<IEntity>();
            Debug.Log($"[ElfFactory] Created entity {obj.name} of type {entityType}");
            return entity;
        }
    }
}