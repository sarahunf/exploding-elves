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
            if (viewModel != null)
            {
                UnsubscribeFromEvents();
            }

            viewModel = new SpawnerUIViewModel(config, spawnerIndex, gameManager);
            SetupUI();
            SubscribeToEvents();
        }

        public void UpdateCount()
        {
            if (viewModel != null)
            {
                viewModel.UpdateCount();
            }
        }

        private void SetupUI()
        {
            if (nameText != null)
            {
                nameText.text = viewModel.SpawnerName;
            }

            if (colorIndicator != null)
            {
                nameText!.color = viewModel.IndicatorColor;
                colorIndicator.color = viewModel.IndicatorColor;
            }

            if (intervalSlider != null)
            {
                float initialValue = Mathf.Clamp(viewModel.SpawnInterval, intervalSlider.minValue, intervalSlider.maxValue);
                intervalSlider.value = initialValue;
                UpdateIntervalText(initialValue);
                intervalSlider.onValueChanged.AddListener(OnIntervalChanged);
            }

            UpdateCountText();
        }

        private void SubscribeToEvents()
        {
            if (viewModel != null)
            {
                viewModel.OnCountChanged += UpdateCountText;
                viewModel.OnIntervalChanged += UpdateIntervalText;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (viewModel != null)
            {
                viewModel.OnCountChanged -= UpdateCountText;
                viewModel.OnIntervalChanged -= UpdateIntervalText;
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void OnIntervalChanged(float value)
        {
            if (viewModel != null)
            {
                viewModel.SetSpawnInterval(value);
            }
        }

        private void UpdateIntervalText(float value)
        {
            if (intervalText != null)
            {
                intervalText.text = $"{value.ToString("F1")}s";
            }
        }

        private void UpdateCountText()
        {
            if (countText != null && viewModel != null)
            {
                countText.text = $"{viewModel.CurrentCount}/{viewModel.MaxEntities}";
            }
        }
    }
} 