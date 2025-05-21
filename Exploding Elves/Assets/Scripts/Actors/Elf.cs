using Actors.Enum;
using Actors.Interface;

namespace Actors
{
    using UnityEngine;
    using System;

    public class Elf : Entity
    {
        [SerializeField] private ParticleSystem explosionEffect;
        [SerializeField] private EntityType type;
        
        public static event Action<EntityType, Vector3> OnElfReplication;

        private void Awake()
        {
            entityType = type;
        }

        private void Update()
        {
            Move();
        }
        
        public override void OnCollision(IEntity other)
        {
            if (other.GetEntityType() == entityType)
            {
                OnElfReplication?.Invoke(entityType, transform.position);
            }
            else
            {
                Explode();
            }
        }

        private void Explode()
        {
            //TODO: replace this with pooling behaviour
            if (explosionEffect != null)
            {
                var explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
                explosion.Play();

                Destroy(explosion.gameObject, explosion.main.duration);
            }
            Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            var otherElf = other.GetComponent<Elf>();
            if (otherElf != null)
            {
                OnCollision(otherElf);
            }
        }
    }
}