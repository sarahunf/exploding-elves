using UnityEngine;
using System;
using Manager;

namespace UI
{
    public class GameViewModel : MonoBehaviour
    {
        private GameStateModel model;
        
        public event Action<bool> OnPauseStateChanged;
        public event Action OnGameRestarted;
        public event Action OnGameQuit;

        private void Awake()
        {
            model = new GameStateModel();
        }

        public void TogglePause()
        {
            if (GameManager.Instance == null) return;
            
            GameManager.Instance.TogglePause();
            model.SetPaused(!model.IsPaused);
            OnPauseStateChanged?.Invoke(model.IsPaused);
        }

        public void RestartGame()
        {
            if (GameManager.Instance == null) return;
            
            GameManager.Instance.RestartGame();
            model.SetPaused(false);
            model.SetGameOver(false);
            OnGameRestarted?.Invoke();
        }

        public void QuitGame()
        {
            if (GameManager.Instance == null) return;
            
            GameManager.Instance.QuitGame();
            OnGameQuit?.Invoke();
        }

        public bool IsPaused() => model.IsPaused;
    }
} 