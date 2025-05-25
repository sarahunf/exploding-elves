using UnityEngine;
using Actors.Enum;

namespace Config
{
    [CreateAssetMenu(fileName = "ElfConfig", menuName = "Config/Elf Config")]
    public class ElfConfig : ScriptableObject
    {
        [Header("Movement Settings")]
        public float moveSpeed = 4f;
        public float randomDirectionChangeInterval = 1f;
        
        [Header("Collision Settings")]
        public float replicationCooldown = 20f;
        public float collisionCleanupInterval = 5f;
        
        [Header("Visual Settings")]
        public ParticleSystem explosionEffect;
        public EntityType type;
        public Color color;
        
        [Header("Pool Settings")]
        public int initialPoolSize = 10;
    }
} 