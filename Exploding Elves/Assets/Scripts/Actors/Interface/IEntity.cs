using Actors.Enum;
using UnityEngine;

namespace Actors.Interface
{
    public interface IEntity
    {
        EntityType GetEntityType();
        void Initialize(Vector3 position);
        void OnCollision(IEntity other);
    }
}