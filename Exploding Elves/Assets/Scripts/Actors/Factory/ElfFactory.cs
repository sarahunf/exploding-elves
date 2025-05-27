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
            if (!poolMap.TryGetValue(entityType, out var pool))
            {
                return null;
            }

            var obj = pool.Get();
            var entity = obj.GetComponent<IEntity>();
            
            if (entity is Elf elf)
            {
                elf.InitializePools(
                    Manager.GameManager.Instance.GetExplosionPool(),
                    Manager.GameManager.Instance.GetSpawningPool()
                );
            }
            
            return entity;
        }
    }
}