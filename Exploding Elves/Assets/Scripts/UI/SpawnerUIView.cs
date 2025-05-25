using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Config;
using Manager;

namespace UI
{
    public class SpawnerUIView : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Slider intervalSlider;
        [SerializeField] private TMP_Text intervalText;
        [SerializeField] private Image colorIndicator;
        [SerializeField] private TMP_Text countText;

        private SpawnerUIViewModel viewModel;

        public void Initialize(SpawnerConfigSO config, int spawnerIndex, GameManager gameManager)
        {
            viewModel = new SpawnerUIViewModel(config, spawnerIndex, gameManager);
            SetupUI();
            SubscribeToEvents();
        }

        private void SetupUI()
        {
            if (nameText != null)
            {
                nameText.text = viewModel.SpawnerName;
            }

            if (colorIndicator != null)
            {
                nameText.color = viewModel.IndicatorColor;
                colorIndicator.color = viewModel.IndicatorColor;
            }

            if (intervalSlider != null)
            {
                intervalSlider.value = viewModel.SpawnInterval;
                UpdateIntervalText(viewModel.SpawnInterval);
                intervalSlider.onValueChanged.AddListener(OnIntervalChanged);
            }

            UpdateCountText();
        }

        private void SubscribeToEvents()
        {
            viewModel.OnCountChanged += UpdateCountText;
            viewModel.OnIntervalChanged += UpdateIntervalText;
        }

        private void OnDestroy()
        {
            if (viewModel != null)
            {
                viewModel.OnCountChanged -= UpdateCountText;
                viewModel.OnIntervalChanged -= UpdateIntervalText;
            }
        }

        private void OnIntervalChanged(float value)
        {
            viewModel.SetSpawnInterval(value);
        }

        private void UpdateIntervalText(float value)
        {
            if (intervalText != null)
            {
                intervalText.text = value.ToString("F1");
            }
        }

        private void UpdateCountText()
        {
            if (countText != null)
            {
                countText.text = $"Count: {viewModel.CurrentCount}/{viewModel.MaxEntities}";
            }
        }
    }
} 