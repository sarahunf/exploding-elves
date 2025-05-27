using UnityEngine;

namespace Actors.Movement
{
    public abstract class MovementStrategySO : ScriptableObject, IMovementStrategy
    {
        [Header("Base Movement Settings")]
        public float directionChangeInterval = 1f;
        
        public abstract Vector3 CalculateMovement(Vector3 currentPosition, Vector3 currentDirection, float moveSpeed, float deltaTime);
        public abstract Vector3 CalculateDirection(Vector3 currentDirection, float deltaTime);
        
        public virtual Quaternion CalculateRotation(Vector3 movement)
        {
            return movement != Vector3.zero ? Quaternion.LookRotation(movement) : Quaternion.identity;
        }
    }
} 