using Actors.Enum;
using Actors.Interface;
using UnityEngine;

namespace Actors
{
    public abstract class Entity : MonoBehaviour, IEntity
    {
        protected EntityType entityType;
    
        public EntityType GetEntityType() => entityType;
    
        public virtual void Initialize(Vector3 position)
        {
            transform.position = position;
        }
    
        public virtual void OnCollision(IEntity other)
        {
            // Base implementation does nothing
        }
    }
}