using Actors.Pool;
using UnityEngine;

namespace Actors.Components
{
    public class ParticleEffectHandler : MonoBehaviour
    {
        private ParticlePool explosionPool;
        private ParticlePool spawningPool;

        public void InitializePools(ParticlePool explosionPool, ParticlePool spawningPool)
        {
            this.explosionPool = explosionPool;
            this.spawningPool = spawningPool;
        }

        public void ShowExplosionEffect()
        {
            if (explosionPool == null) return;
            
            var explosion = explosionPool.Get();
            if (explosion == null) return;
                
            explosion.transform.position = transform.position;
            var ps = explosion.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                ps.Play();
                explosionPool.ReturnToPoolAfterDuration(explosion, (main.duration + main.startDelay.constant) + 0.2f);
            }
        }

        public void ShowSpawnEffect(Vector3 position)
        {
            if (spawningPool == null) return;
            
            var spawnEffect = spawningPool.Get();
            if (spawnEffect == null) return;
                
            spawnEffect.transform.position = position;
            var ps = spawnEffect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                ps.Play();
                spawningPool.ReturnToPoolAfterDuration(spawnEffect, (main.duration + main.startDelay.constant) + 0.2f);
            }
        }
    }
} 