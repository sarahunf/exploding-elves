using UnityEngine;

namespace UI
{
    public class GameStateModel
    {
        public bool IsPaused { get; private set; }
        public bool IsGameOver { get; private set; }

        public void SetPaused(bool isPaused)
        {
            IsPaused = isPaused;
        }

        public void SetGameOver(bool isGameOver)
        {
            IsGameOver = isGameOver;
        }
    }
} 