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
            public string spawnerName;
            public Slider intervalSlider;
            public TMP_Text intervalText;
            public Image colorIndicator;
        }
    
        [SerializeField] private SpawnerUI[] spawnerUIs;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private Color[] elfColors; // Matched to EntityType enum order
    
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
                int spawnerIndex = i;
                
                UpdateIntervalText(spawnerUIs[i].intervalText, spawnerUIs[i].intervalSlider.value);
                
                if (i < elfColors.Length)
                {
                    spawnerUIs[i].colorIndicator.color = elfColors[i];
                }
                
                spawnerUIs[i].intervalSlider.onValueChanged.AddListener((value) => 
                {
                    UpdateIntervalText(spawnerUIs[spawnerIndex].intervalText, value);
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