using UnityEngine;

namespace Actors.Movement
{
    [CreateAssetMenu(fileName = "ErraticMovementStrategy", menuName = "Movement/Erratic Movement Strategy")]
    public class ErraticMovementStrategySO : MovementStrategySO
    {
        [Header("Erratic Movement Settings")]
        public float randomDirectionStrength = 1f;
        
        [Header("Wall Detection Settings")]
        [Tooltip("How far to check for obstacles")]
        public float raycastDistance = 1f;
        
        public override IMovementStrategy.MovementResult CalculateMovement(Vector3 currentPosition, Vector3 currentDirection, float moveSpeed, float deltaTime)
        {
            bool isNearRock = Physics.Raycast(currentPosition + Vector3.up * 0.1f, currentDirection, out var hit, raycastDistance) && hit.collider.CompareTag("Rock");
            if (isNearRock)
            {
                currentDirection = -currentDirection;
            }
            
            var horizontalMovement = new Vector3(currentDirection.x, 0, currentDirection.z) * (moveSpeed * deltaTime);
            var newPosition = currentPosition + horizontalMovement;
            
            if (Physics.Raycast(newPosition + Vector3.up * 0.1f, Vector3.zero, out var finalHit, 0.1f) && finalHit.collider.CompareTag("Rock"))
            {
                return new IMovementStrategy.MovementResult(currentPosition, currentDirection);
            }
            
            return new IMovementStrategy.MovementResult(newPosition, currentDirection);
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