using Actors.Pool;
using UnityEngine;

namespace Manager
{
    public class ParticleManager : MonoBehaviour
    {
        [SerializeField] private ParticlePool explosionPool;
        [SerializeField] private ParticlePool spawningPool;

        public ParticlePool GetExplosionPool() => explosionPool;
        public ParticlePool GetSpawningPool() => spawningPool;
    }
} 