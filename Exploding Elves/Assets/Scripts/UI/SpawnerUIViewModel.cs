using System;
using Config;
using Manager;
using Actors.Enum;

namespace UI
{
    public class SpawnerUIViewModel
    {
        private readonly SpawnerConfigSO config;
        private readonly int spawnerIndex;
        private readonly GameManager gameManager;
        private int currentCount;

        public event Action OnCountChanged;
        public event Action<float> OnIntervalChanged;

        public string SpawnerName => config.spawnerName;
        public UnityEngine.Color IndicatorColor => config.indicatorColor;
        public float SpawnInterval => config.spawnInterval;
        public int CurrentCount => currentCount;
        public int MaxEntities => config.maxEntities;
        public EntityType EntityType => config.entityType;

        public SpawnerUIViewModel(SpawnerConfigSO config, int spawnerIndex, GameManager gameManager)
        {
            this.config = config;
            this.spawnerIndex = spawnerIndex;
            this.gameManager = gameManager;
            UpdateCount();
        }

        public void SetSpawnInterval(float value)
        {
            gameManager.SetSpawnerInterval(spawnerIndex, value, true);
            OnIntervalChanged?.Invoke(value);
        }

        public void UpdateCount()
        {
            currentCount = EntityCounter.Instance.GetEntityCount(config.entityType);
            OnCountChanged?.Invoke();
        }
    }
} 