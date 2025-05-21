using Actors.Enum;
using Actors.Interface;
using UnityEngine;

namespace Actors
{
    public abstract class Entity : MonoBehaviour, IEntity
    {
        [SerializeField] protected float moveSpeed = 2f;
        [SerializeField] protected float randomDirectionChangeInterval = 2f;
    
        protected Vector3 moveDirection;
        protected EntityType entityType;
        protected float directionChangeTimer;
    
        public virtual void Initialize(Vector3 position)
        {
            transform.position = position;
            ChangeDirection();
        }
    
        public virtual void Move()
        {
            directionChangeTimer -= Time.deltaTime;
            if (directionChangeTimer <= 0)
            {
                ChangeDirection();
            }
        
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        
            // Keep within bounds (simple boundary check)
            Vector3 pos = transform.position;
            float boundary = 10f; // Assuming a 10x10 play area
        
            if (Mathf.Abs(pos.x) > boundary || Mathf.Abs(pos.z) > boundary)
            {
                moveDirection = -moveDirection; // Bounce off the boundary
                transform.position = new Vector3(
                    Mathf.Clamp(pos.x, -boundary, boundary),
                    pos.y,
                    Mathf.Clamp(pos.z, -boundary, boundary)
                );
            }
        }
    
        protected virtual void ChangeDirection()
        {
            moveDirection = new Vector3(
                UnityEngine.Random.Range(-1f, 1f),
                0,
                UnityEngine.Random.Range(-1f, 1f)
            ).normalized;
        
            directionChangeTimer = randomDirectionChangeInterval;
        }
    
        public abstract void OnCollision(IEntity other);
    
        public EntityType GetEntityType()
        {
            return entityType;
        }
    }
}