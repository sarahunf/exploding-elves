using UnityEngine;

namespace Manager
{
    public class SpawnManager : MonoBehaviour
    {
        [SerializeField] private Spawner.Spawner[] spawners;

        public void SetSpawnerInterval(int spawnerIndex, float interval, bool resetTimer)
        {
            if (spawnerIndex >= 0 && spawnerIndex < spawners.Length)
            {
                spawners[spawnerIndex].SetSpawnInterval(interval);
            }
        }

        public Spawner.Spawner[] GetSpawners() => spawners;
    }
} 