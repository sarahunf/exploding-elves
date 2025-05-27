using UnityEngine;
using Actors.Enum;

namespace Actors.Movement
{
    [CreateAssetMenu(fileName = "AttackMovementStrategy", menuName = "Movement/Attack Movement Strategy")]
    public class AttackMovementStrategySO : MovementStrategySO
    {
        [Header("Attack Movement Settings")]
        [Tooltip("Speed when charging at target")]
        public float chargeSpeed = 8f;
        
        [Tooltip("Duration of charge attack")]
        public float chargeDuration = 1f;
        
        [Tooltip("Duration of cooldown between charges")]
        public float cooldownDuration = 2f;
        
        [Tooltip("Radius to detect potential targets")]
        public float detectionRadius = 15f;
        
        [Tooltip("Type of elf to target")]
        public EntityType targetType;

        [Header("Default Movement Settings")]
        [Tooltip("Movement strategy to use when no targets are found")]
        public MovementStrategySO defaultStrategy;
        
        [Tooltip("How long to wait before switching to default strategy when no targets are found")]
        public float defaultStrategyDelay = 3f;
        
        private float currentChargeTime;
        private bool isCharging;
        private Elf currentTarget;
        private Vector3 lastKnownTargetPosition;
        private float targetSearchCooldown;
        private float noTargetTimer;
        private const float TARGET_SEARCH_INTERVAL = 0.5f;
        
        public override IMovementStrategy.MovementResult CalculateMovement(Vector3 currentPosition, Vector3 currentDirection, float moveSpeed, float deltaTime)
        {
            UpdateTargetSearch(deltaTime);
            UpdateTargetStatus();
            
            if (currentTarget == null && lastKnownTargetPosition == Vector3.zero)
            {
                noTargetTimer += deltaTime;
                if (noTargetTimer >= defaultStrategyDelay && defaultStrategy != null)
                {
                    var result = defaultStrategy.CalculateMovement(currentPosition, currentDirection, moveSpeed, deltaTime);
                    return new IMovementStrategy.MovementResult(result.position, result.direction);
                }
            }
            else
            {
                noTargetTimer = 0f;
            }
            
            var targetDirection = GetTargetDirection(currentPosition, currentDirection);
            float currentSpeed = isCharging ? chargeSpeed : moveSpeed;
            
            UpdateChargeState(deltaTime);
            
            var newPosition = currentPosition + targetDirection * (currentSpeed * deltaTime);
            return new IMovementStrategy.MovementResult(newPosition, targetDirection);
        }
        
        private void UpdateTargetSearch(float deltaTime)
        {
            targetSearchCooldown -= deltaTime;
            if (targetSearchCooldown <= 0)
            {
                FindNewTarget(Vector3.zero);
                targetSearchCooldown = TARGET_SEARCH_INTERVAL;
            }
        }
        
        private void UpdateTargetStatus()
        {
            if (currentTarget != null)
            {
                if (currentTarget.gameObject.activeInHierarchy)
                {
                    lastKnownTargetPosition = currentTarget.transform.position;
                }
                else
                {
                    currentTarget = null;
                }
            }
        }
        
        private void UpdateChargeState(float deltaTime)
        {
            currentChargeTime += deltaTime;
            float duration = isCharging ? chargeDuration : cooldownDuration;
            
            if (currentChargeTime >= duration)
            {
                isCharging = !isCharging;
                currentChargeTime = 0f;
            }
        }
        
        private void FindNewTarget(Vector3 currentPosition)
        {
            var nearbyColliders = Physics.OverlapSphere(currentPosition, detectionRadius);
            float closestDistance = float.MaxValue;
            Elf closestTarget = null;

            foreach (var collider in nearbyColliders)
            {
                var elf = collider.GetComponent<Elf>();
                if (elf != null && elf.GetEntityType() == targetType)
                {
                    float distance = Vector3.Distance(currentPosition, elf.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = elf;
                    }
                }
            }

            if (closestTarget != null)
            {
                currentTarget = closestTarget;
                lastKnownTargetPosition = closestTarget.transform.position;
                noTargetTimer = 0f;
            }
        }
        
        private Vector3 GetTargetDirection(Vector3 currentPosition, Vector3 currentDirection)
        {
            if (currentTarget != null && currentTarget.gameObject.activeInHierarchy)
            {
                return (currentTarget.transform.position - currentPosition).normalized;
            }
            
            if (lastKnownTargetPosition != Vector3.zero)
            {
                var direction = (lastKnownTargetPosition - currentPosition).normalized;
                if (Vector3.Distance(currentPosition, lastKnownTargetPosition) < 0.5f)
                {
                    lastKnownTargetPosition = Vector3.zero;
                    return currentDirection;
                }
                return direction;
            }
            
            return currentDirection;
        }
        
        public override Vector3 CalculateDirection(Vector3 currentDirection, float deltaTime)
        {
            if (currentTarget == null && lastKnownTargetPosition == Vector3.zero && defaultStrategy != null)
            {
                return defaultStrategy.CalculateDirection(currentDirection, deltaTime);
            }
            return GetTargetDirection(Vector3.zero, currentDirection);
        }
        
        public override Quaternion CalculateRotation(Vector3 movement)
        {
            return movement != Vector3.zero ? Quaternion.LookRotation(movement) : Quaternion.identity;
        }
    }
} 