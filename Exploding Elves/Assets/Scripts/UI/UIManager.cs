using Actors.Enum;
using AYellowpaper.SerializedCollections;
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
            public EntityType entityType;
            public string spawnerName;
            public Slider intervalSlider;
            public TMP_Text intervalText;
            public Image colorIndicator;
        }
    
        [SerializeField] private SpawnerUI[] spawnerUIs;
        [SerializeField] private GameManager gameManager;
        [SerializedDictionary("Entity Type", "Color")]
        [SerializeField] private SerializedDictionary<EntityType, Color> elfColorDict;
    
        private void Start()
        {
            InitializeUI();
        }
    
        private void InitializeUI()
        {
            if (gameManager == null)
            {
                gameManager = FindObjectOfType<GameManager>();
                if (gameManager == null)
                {
                    Debug.LogError("GameManager not found in the scene!");
                    return;
                }
            }
        
            for (int i = 0; i < spawnerUIs.Length; i++)
            {
                var spawner = spawnerUIs[i];
                int spawnerIndex = i;

                UpdateIntervalText(spawner.intervalText, spawner.intervalSlider.value);

                if (elfColorDict.TryGetValue(spawner.entityType, out var color))
                    spawner.colorIndicator.color = color;
                else
                    Debug.LogWarning($"No color defined for {spawner.entityType} in UIManager.");

                spawner.intervalSlider.onValueChanged.AddListener((value) =>
                {
                    UpdateIntervalText(spawner.intervalText, value);
                    gameManager.SetSpawnerInterval(spawnerIndex, value);
                });
            }
        }
    
        private void UpdateIntervalText(TMP_Text text, float value)
        {
            text.text = value.ToString("F1") + "s";
        }
    }
}