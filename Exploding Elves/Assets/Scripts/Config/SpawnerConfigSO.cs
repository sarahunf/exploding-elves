using UnityEngine;
using Actors.Enum;

namespace Config
{
    [CreateAssetMenu(fileName = "SpawnerConfig", menuName = "Config/Spawner Config")]
    public class SpawnerConfigSO : ScriptableObject
    {
        [Header("Spawn Settings")]
        public EntityType entityType;
        public float spawnInterval = 2f;
        public Vector3 spawnAreaSize = new Vector3(5f, 0, 5f);
        
        [Header("Limit Settings")]
        public int maxEntities = 100;
        
        [Header("UI Settings")]
        public string spawnerName;
        public Color indicatorColor = Color.white;
    }
} 