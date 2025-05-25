using UnityEngine;

namespace Actors.Movement
{
    public interface IMovementStrategy
    {
        Vector3 CalculateMovement(Vector3 currentPosition, Vector3 currentDirection, float moveSpeed, float deltaTime);
        Vector3 CalculateDirection(Vector3 currentDirection, float deltaTime);
        Quaternion CalculateRotation(Vector3 movement);
    }
} 