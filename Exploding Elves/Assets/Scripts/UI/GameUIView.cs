using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    public class GameUIView : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;
        
        [Header("Buttons")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button quitButton;
        
        [Header("Game Over UI")]
        [SerializeField] private TextMeshProUGUI gameOverText;
        
        private GameViewModel viewModel;
        
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
            // Hide panels initially
            if (pausePanel) pausePanel.SetActive(false);
            if (gameOverPanel) gameOverPanel.SetActive(false);
        }

        private void SetupEventListeners()
        {
            // Button listeners
            if (pauseButton) pauseButton.onClick.AddListener(OnPauseButtonClicked);
            if (resumeButton) resumeButton.onClick.AddListener(OnResumeButtonClicked);
            if (restartButton) restartButton.onClick.AddListener(OnRestartButtonClicked);
            if (quitButton) quitButton.onClick.AddListener(OnQuitButtonClicked);
            
            // ViewModel event listeners
            viewModel.OnPauseStateChanged += HandlePauseStateChanged;
            viewModel.OnGameRestarted += HandleGameRestarted;
            viewModel.OnGameQuit += HandleGameQuit;
        }

        private void Update()
        {
            // Handle escape key for pause
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                viewModel.TogglePause();
            }
        }

        private void OnPauseButtonClicked()
        {
            viewModel.TogglePause();
        }

        private void OnResumeButtonClicked()
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

        private void HandlePauseStateChanged(bool isPaused)
        {
            if (pausePanel) pausePanel.SetActive(isPaused);
        }

        private void HandleGameRestarted()
        {
            if (pausePanel) pausePanel.SetActive(false);
            if (gameOverPanel) gameOverPanel.SetActive(false);
        }

        private void HandleGameQuit()
        {
            // Any cleanup needed before quitting
        }

        private void OnDestroy()
        {
            // Clean up event listeners
            if (pauseButton) pauseButton.onClick.RemoveListener(OnPauseButtonClicked);
            if (resumeButton) resumeButton.onClick.RemoveListener(OnResumeButtonClicked);
            if (restartButton) restartButton.onClick.RemoveListener(OnRestartButtonClicked);
            if (quitButton) quitButton.onClick.RemoveListener(OnQuitButtonClicked);
            
            if (viewModel != null)
            {
                viewModel.OnPauseStateChanged -= HandlePauseStateChanged;
                viewModel.OnGameRestarted -= HandleGameRestarted;
                viewModel.OnGameQuit -= HandleGameQuit;
            }
        }
    }
} 