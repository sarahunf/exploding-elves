using UnityEngine;

namespace Actors.Movement
{
    [CreateAssetMenu(fileName = "ErraticMovementStrategy", menuName = "Movement/Erratic Movement Strategy")]
    public class ErraticMovementStrategySO : MovementStrategySO
    {
        [Header("Erratic Movement Settings")]
        public float randomDirectionStrength = 1f;
        
        public override Vector3 CalculateMovement(Vector3 currentPosition, Vector3 currentDirection, float moveSpeed, float deltaTime)
        {
            Vector3 horizontalMovement = new Vector3(currentDirection.x, 0, currentDirection.z) * (moveSpeed * deltaTime);
            return currentPosition + horizontalMovement;
        }
        
        public override Vector3 CalculateDirection(Vector3 currentDirection, float deltaTime)
        {
            return new Vector3(
                Random.Range(-randomDirectionStrength, randomDirectionStrength),
                0,
                Random.Range(-randomDirectionStrength, randomDirectionStrength)
            ).normalized;
        }
    }
} 