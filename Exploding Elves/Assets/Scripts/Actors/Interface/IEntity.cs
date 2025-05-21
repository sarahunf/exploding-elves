using Actors.Enum;
using UnityEngine;

// Main entity interface
namespace Actors.Interface
{
    public interface IEntity
    {
        void Initialize(Vector3 position);
        void Move();
        void OnCollision(IEntity other);
        EntityType GetEntityType();
    }
}