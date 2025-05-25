using System.Collections.Generic;
using Actors.Enum;

namespace Manager
{
    public class EntityCounter
    {
        private static EntityCounter instance;
        private Dictionary<EntityType, int> entityCounts = new Dictionary<EntityType, int>();

        public static EntityCounter Instance => instance ??= new EntityCounter();

        private EntityCounter() { }

        public bool CanSpawnEntity(EntityType type, int maxCount)
        {
            entityCounts.TryAdd(type, 0);
            return entityCounts[type] < maxCount;
        }

        public void OnEntitySpawned(EntityType type)
        {
            entityCounts.TryAdd(type, 0);
            entityCounts[type]++;
        }

        public void OnEntityDestroyed(EntityType type)
        {
            if (!entityCounts.ContainsKey(type))
                return;
            
            entityCounts[type]--;
            if (entityCounts[type] < 0)
            {
                entityCounts[type] = 0;
            }
        }
    }
} 