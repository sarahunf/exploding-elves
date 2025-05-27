using UnityEngine;

namespace Actors.Movement
{
    public interface IMovementStrategy
    {
        public struct MovementResult
        {
            public Vector3 position;
            public Vector3 direction;
            public MovementResult(Vector3 position, Vector3 direction)
            {
                this.position = position;
                this.direction = direction;
            }
        }
        MovementResult CalculateMovement(Vector3 currentPosition, Vector3 currentDirection, float moveSpeed, float deltaTime);
        Vector3 CalculateDirection(Vector3 currentDirection, float deltaTime);
        Quaternion CalculateRotation(Vector3 movement);
    }
} 