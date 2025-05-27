using Actors;
using Actors.Enum;
using Config;
using Manager;
using UnityEngine;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [System.Serializable]
        public class SpawnerUIConfig
        {
            public SpawnerConfigSO config;
            public SpawnerUIView view;
        }
    
        [SerializeField] private SpawnerUIConfig[] spawnerConfigs;
        [SerializeField] private GameManager gameManager;
    
        public void SetGameManager(GameManager manager)
        {
            gameManager = manager;
            InitializeUI();
        }

        private void OnEnable()
        {
            Spawner.Spawner.OnEntitySpawned += HandleEntitySpawned;
            Elf.OnEntityDestroyed += HandleEntityDestroyed;
        }

        private void OnDisable()
        {
            Spawner.Spawner.OnEntitySpawned -= HandleEntitySpawned;
            Elf.OnEntityDestroyed -= HandleEntityDestroyed;
        }

        private void HandleEntitySpawned(EntityType type)
        {
            UpdateCountForType(type);
        }

        private void HandleEntityDestroyed(EntityType type)
        {
            UpdateCountForType(type);
        }

        private void UpdateCountForType(EntityType type)
        {
            foreach (var config in spawnerConfigs)
            {
                if (config.config == null || config.view == null || config.config.entityType != type) continue;
                config.view.UpdateCount();
            }
        }
    
        private void InitializeUI()
        {
            if (gameManager == null)
            {
                Debug.LogError("GameManager not found! Please set it using SetGameManager.");
                return;
            }
        
            for (int i = 0; i < spawnerConfigs.Length; i++)
            {
                var config = spawnerConfigs[i];
                if (config.config == null || config.view == null) continue;

                config.view.Initialize(config.config, i, gameManager);
            }
        }
    }
}