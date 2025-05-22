using Actors.Enum;
using Actors.Interface;
using UnityEngine;

namespace Actors
{
    public abstract class Entity : MonoBehaviour, IEntity
    {
        [SerializeField] protected float moveSpeed = 2f;
        [SerializeField] protected float randomDirectionChangeInterval = 2f;
        [SerializeField] protected float boundaryArea = 50f;
    
        protected Vector3 currentDirection;
        protected EntityType entityType;
        protected float nextDirectionChangeTime;
    
        public EntityType GetEntityType() => entityType;
    
        public virtual void Initialize(Vector3 position)
        {
            transform.position = position;
            currentDirection = Vector3.zero;
            nextDirectionChangeTime = 0f;
        }
    
        public virtual void OnCollision(IEntity other)
        {
            // Base implementation does nothing
        }
    
        protected abstract void Move();
    
        protected virtual void ChangeDirection()
        {
            currentDirection = new Vector3(
                Random.Range(-1f, 1f),
                0,
                Random.Range(-1f, 1f)
            ).normalized;
        
            nextDirectionChangeTime = randomDirectionChangeInterval;
        }
    }
}