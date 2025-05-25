using Actors;
using Actors.Enum;
using Config;
using Manager;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [System.Serializable]
        public class SpawnerUI
        {
            [FormerlySerializedAs("config")] public SpawnerConfigSO _configSo;
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
                if (ui._configSo == null || ui.countText == null || ui._configSo.entityType != type) continue;

                int count = Manager.EntityCounter.Instance.GetEntityCount(type);
                ui.countText.text = $"Count: {count}/{ui._configSo.maxEntities}";
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
                if (ui._configSo == null) continue;

                // Set name
                if (ui.nameText != null)
                {
                    ui.nameText.text = ui._configSo.spawnerName;
                }

                // Set color indicator
                if (ui.colorIndicator != null)
                {
                    ui.colorIndicator.color = ui._configSo.indicatorColor;
                }

                // Set up interval slider
                if (ui.intervalSlider != null)
                {
                    ui.intervalSlider.value = ui._configSo.spawnInterval;
                    if (ui.intervalText != null)
                    {
                        ui.intervalText.text = ui._configSo.spawnInterval.ToString("F1");
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
                    int count = EntityCounter.Instance.GetEntityCount(ui._configSo.entityType);
                    ui.countText.text = $"{count}/{ui._configSo.maxEntities}";
                }
            }
        }
    }
}