using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    public class GameUIView : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject pausePanel;
        
        [Header("Buttons")]
        [SerializeField] private Button playPauseButton;
        [SerializeField] private TextMeshProUGUI playPauseButtonText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button togglePanelButton;
        
        private GameViewModel viewModel;
        private bool isPanelOpen = false;
        
        private void Awake()
        {
            viewModel = GetComponent<GameViewModel>();
            if (viewModel == null)
            {
                viewModel = gameObject.AddComponent<GameViewModel>();
            }
            
            SetupUI();
            SetupEventListeners();
        }

        private void SetupUI()
        {
            // Hide panel initially
            if (pausePanel) pausePanel.SetActive(false);
            
            // Set initial button texts
            UpdatePlayPauseButtonText(false);
        }

        private void SetupEventListeners()
        {
            // Button listeners
            if (playPauseButton) playPauseButton.onClick.AddListener(OnPlayPauseButtonClicked);
            if (restartButton) restartButton.onClick.AddListener(OnRestartButtonClicked);
            if (quitButton) quitButton.onClick.AddListener(OnQuitButtonClicked);
            if (togglePanelButton) togglePanelButton.onClick.AddListener(OnTogglePanelButtonClicked);
            
            // ViewModel event listeners
            viewModel.OnPauseStateChanged += HandlePauseStateChanged;
            viewModel.OnGameRestarted += HandleGameRestarted;
            viewModel.OnGameQuit += HandleGameQuit;
        }

        private void Update()
        {
            // Handle escape key for panel toggle
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePanel();
            }
        }

        private void OnPlayPauseButtonClicked()
        {
            viewModel.TogglePause();
        }

        private void OnRestartButtonClicked()
        {
            viewModel.RestartGame();
        }

        private void OnQuitButtonClicked()
        {
            viewModel.QuitGame();
        }

        private void OnTogglePanelButtonClicked()
        {
            TogglePanel();
        }

        private void TogglePanel()
        {
            isPanelOpen = !isPanelOpen;
            if (pausePanel) pausePanel.SetActive(isPanelOpen);
        }


        private void HandlePauseStateChanged(bool isPaused)
        {
            UpdatePlayPauseButtonText(isPaused);
        }

        private void UpdatePlayPauseButtonText(bool isPaused)
        {
            if (playPauseButtonText != null)
            {
                playPauseButtonText.text = isPaused ? "Play" : "Pause";
            }
        }

        private void HandleGameRestarted()
        {
            if (pausePanel) pausePanel.SetActive(false);
            isPanelOpen = false;
            UpdatePlayPauseButtonText(false);
        }

        private void HandleGameQuit()
        {
            // Any cleanup needed before quitting
        }

        private void OnDestroy()
        {
            // Clean up event listeners
            if (playPauseButton) playPauseButton.onClick.RemoveListener(OnPlayPauseButtonClicked);
            if (restartButton) restartButton.onClick.RemoveListener(OnRestartButtonClicked);
            if (quitButton) quitButton.onClick.RemoveListener(OnQuitButtonClicked);
            if (togglePanelButton) togglePanelButton.onClick.RemoveListener(OnTogglePanelButtonClicked);
            
            if (viewModel != null)
            {
                viewModel.OnPauseStateChanged -= HandlePauseStateChanged;
                viewModel.OnGameRestarted -= HandleGameRestarted;
                viewModel.OnGameQuit -= HandleGameQuit;
            }
        }
    }
} 