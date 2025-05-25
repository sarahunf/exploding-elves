using Actors;
using Actors.Enum;
using Config;
using Manager;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [System.Serializable]
        public class SpawnerUI
        {
            public SpawnerConfig config;
            public TMP_Text nameText;
            public Slider intervalSlider;
            public TMP_Text intervalText;
            public Image colorIndicator;
            public TMP_Text countText;
        }
    
        [SerializeField] private SpawnerUI[] spawnerUIs;
        [SerializeField] private GameManager gameManager;
    
        private void Awake()
        {
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                return;
            }
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
            foreach (var ui in spawnerUIs)
            {
                if (ui.config == null || ui.countText == null || ui.config.entityType != type) continue;

                int count = Manager.EntityCounter.Instance.GetEntityCount(type);
                ui.countText.text = $"Count: {count}/{ui.config.maxEntities}";
            }
        }
    
        private void Start()
        {
            InitializeUI();
        }
    
        private void InitializeUI()
        {
            if (gameManager == null)
            {
                Debug.LogError("GameManager not found in the scene!");
                return;
            }
        
            for (int i = 0; i < spawnerUIs.Length; i++)
            {
                var ui = spawnerUIs[i];
                if (ui.config == null) continue;

                // Set name
                if (ui.nameText != null)
                {
                    ui.nameText.text = ui.config.spawnerName;
                }

                // Set color indicator
                if (ui.colorIndicator != null)
                {
                    ui.colorIndicator.color = ui.config.indicatorColor;
                }

                // Set up interval slider
                if (ui.intervalSlider != null)
                {
                    ui.intervalSlider.value = ui.config.spawnInterval;
                    if (ui.intervalText != null)
                    {
                        ui.intervalText.text = ui.config.spawnInterval.ToString("F1");
                    }

                    int spawnerIndex = i; // Capture for lambda
                    ui.intervalSlider.onValueChanged.AddListener((value) =>
                    {
                        gameManager.SetSpawnerInterval(spawnerIndex, value);
                        if (ui.intervalText != null)
                        {
                            ui.intervalText.text = value.ToString("F1");
                        }
                    });
                }

                // Initialize count text
                if (ui.countText != null)
                {
                    int count = EntityCounter.Instance.GetEntityCount(ui.config.entityType);
                    ui.countText.text = $"{count}/{ui.config.maxEntities}";
                }
            }
        }
    }
}