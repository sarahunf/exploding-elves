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
            
            Vector3 pos = transform.position;
        
            if (Mathf.Abs(pos.x) > boundaryArea || Mathf.Abs(pos.z) > boundaryArea)
            {
                moveDirection = -moveDirection;
                transform.position = new Vector3(
                    Mathf.Clamp(pos.x, -boundaryArea, boundaryArea),
                    pos.y,
                    Mathf.Clamp(pos.z, -boundaryArea, boundaryArea)
                );
            }
        }
    
        protected virtual void ChangeDirection()
        {
            moveDirection = new Vector3(
                Random.Range(-1f, 1f),
                0,
                Random.Range(-1f, 1f)
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