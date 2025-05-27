using UnityEngine;
using UnityEngine.SceneManagement;
using UI;
using Actors.Pool;
namespace Manager
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private UIManager uiManager;
        [SerializeField] private ElfReplicationManager replicationManager;
        [SerializeField] private ParticleManager particleManager;
        [SerializeField] private SpawnManager spawnManager;

        private static GameManager instance;
        public static GameManager Instance => instance;

        private bool isPaused;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            InitializeManagers();
        }

        private void InitializeManagers()
        {
            if (uiManager != null)
            {
                uiManager.SetGameManager(this);
            }
            else
            {
                Debug.LogError("UIManager reference not set in the inspector!");
            }

            if (particleManager == null)
            {
                Debug.LogError("ParticleManager reference not set in the inspector!");
            }

            if (spawnManager == null)
            {
                Debug.LogError("SpawnManager reference not set in the inspector!");
            }
        }

        public void PauseGame()
        {
            isPaused = true;
            Time.timeScale = 0f;
        }

        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1f;
        }

        public void TogglePause()
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void QuitGame()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        
        public ParticlePool GetExplosionPool() => particleManager?.GetExplosionPool();
        public ParticlePool GetSpawningPool() => particleManager?.GetSpawningPool();
        
        public void SetSpawnerInterval(int spawnerIndex, float interval, bool resetTimer)
        {
            spawnManager?.SetSpawnerInterval(spawnerIndex, interval, resetTimer);
        }
    }
}