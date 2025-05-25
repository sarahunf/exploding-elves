using UnityEngine;
using System.Collections.Generic;
using Actors;

namespace Actors.Movement
{
    [CreateAssetMenu(fileName = "ZigZagMovementStrategy", menuName = "Movement/ZigZag Movement Strategy")]
    public class ZigZagMovementStrategySO : MovementStrategySO
    {
        [Header("ZigZag Movement Settings")]
        [Tooltip("Angle of zigzag in degrees")]
        public float zigzagAngle = 45f;
        
        [Tooltip("How far to check for obstacles")]
        public float raycastDistance = 1f;
        
        [Tooltip("Layer mask for obstacles to detect")]
        public LayerMask obstacleLayer;
        
        [Tooltip("Distance to maintain from walls")]
        public float wallAvoidanceDistance = 0.5f;
        
        [Tooltip("How strongly to correct position when stuck")]
        public float positionCorrectionStrength = 5f;
        
        // Dictionary to store instance-specific data
        private Dictionary<int, ZigZagData> instanceData = new Dictionary<int, ZigZagData>();
        
        private class ZigZagData
        {
            public bool isZigging = true;
            public Vector3 baseDirection;
            public float currentZigzagAngle;
            public bool hasInitialized = false;
            public Vector3 lastSafePosition;
            public float stuckTimer;
        }
        
        private ZigZagData GetInstanceData(Elf elf)
        {
            int instanceId = elf.GetInstanceID();
            if (!instanceData.ContainsKey(instanceId))
            {
                instanceData[instanceId] = new ZigZagData();
            }
            return instanceData[instanceId];
        }
        
        public override Vector3 CalculateMovement(Vector3 currentPosition, Vector3 currentDirection, float moveSpeed, float deltaTime)
        {
            // Find the Elf component in the scene
            Elf elf = FindElfAtPosition(currentPosition);
            if (elf == null) return currentPosition;

            // Get instance-specific data
            var data = GetInstanceData(elf);
            
            // Initialize base direction and angle if not set
            if (!data.hasInitialized)
            {
                data.baseDirection = new Vector3(
                    Random.Range(-1f, 1f),
                    0,
                    Random.Range(-1f, 1f)
                ).normalized;
                
                // Set initial random angle only once
                data.currentZigzagAngle = Random.Range(0f, 360f);
                data.hasInitialized = true;
                data.lastSafePosition = currentPosition;
            }
            
            // Check for obstacles ahead
            Vector3 currentZigzagDirection = GetZigzagDirection(data);
            RaycastHit hit;
            bool isNearWall = Physics.Raycast(currentPosition + Vector3.up * 0.1f, currentZigzagDirection, out hit, raycastDistance, obstacleLayer);
            
            if (isNearWall)
            {
                data.isZigging = !data.isZigging;
                
                // Calculate new base direction after hit
                Vector3 hitNormal = hit.normal;
                data.baseDirection = Vector3.Reflect(currentZigzagDirection, hitNormal);
                data.baseDirection.y = 0;
                data.baseDirection.Normalize();
                
                // Move away from wall
                currentPosition += hitNormal * wallAvoidanceDistance;
            }
            
            // Check if we're stuck
            float distanceMoved = Vector3.Distance(currentPosition, data.lastSafePosition);
            if (distanceMoved < 0.01f)
            {
                data.stuckTimer += deltaTime;
                if (data.stuckTimer > 0.5f) // If stuck for more than 0.5 seconds
                {
                    // Try to move away from walls in all directions
                    Vector3 escapeDirection = Vector3.zero;
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = i * 45f * Mathf.Deg2Rad;
                        Vector3 testDirection = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                        if (!Physics.Raycast(currentPosition + Vector3.up * 0.1f, testDirection, wallAvoidanceDistance, obstacleLayer))
                        {
                            escapeDirection = testDirection;
                            break;
                        }
                    }
                    
                    if (escapeDirection != Vector3.zero)
                    {
                        // Move in the escape direction
                        currentPosition += escapeDirection * (moveSpeed * deltaTime * positionCorrectionStrength);
                        data.baseDirection = escapeDirection;
                        data.currentZigzagAngle = Mathf.Atan2(escapeDirection.z, escapeDirection.x) * Mathf.Rad2Deg;
                    }
                    else
                    {
                        // If no escape direction found, try to teleport to last safe position
                        currentPosition = data.lastSafePosition;
                    }
                    
                    data.stuckTimer = 0f;
                }
            }
            else
            {
                data.stuckTimer = 0f;
                data.lastSafePosition = currentPosition;
            }
            
            // Apply movement
            Vector3 horizontalMovement = currentZigzagDirection * (moveSpeed * deltaTime);
            Vector3 newPosition = currentPosition + horizontalMovement;
            
            // Final wall check
            if (Physics.Raycast(newPosition + Vector3.up * 0.1f, Vector3.zero, 0.1f, obstacleLayer))
            {
                return currentPosition; // Don't move if we would end up in a wall
            }
            
            return newPosition;
        }
        
        private Elf FindElfAtPosition(Vector3 position)
        {
            // Find all elves in the scene
            var elves = FindObjectsOfType<Elf>();
            Elf closestElf = null;
            float closestDistance = float.MaxValue;

            foreach (var elf in elves)
            {
                float distance = Vector3.Distance(elf.transform.position, position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestElf = elf;
                }
            }

            return closestElf;
        }
        
        public override Vector3 CalculateDirection(Vector3 currentDirection, float deltaTime)
        {
            // Find the Elf component in the scene
            Elf elf = FindElfAtPosition(Vector3.zero);
            if (elf == null) return currentDirection;

            // Get instance-specific data
            var data = GetInstanceData(elf);
            return GetZigzagDirection(data);
        }
        
        private Vector3 GetZigzagDirection(ZigZagData data)
        {
            float angle = data.isZigging ? data.currentZigzagAngle + zigzagAngle : data.currentZigzagAngle - zigzagAngle;
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            return rotation * data.baseDirection;
        }
        
        public override Quaternion CalculateRotation(Vector3 movement)
        {
            if (movement != Vector3.zero)
            {
                return Quaternion.LookRotation(movement);
            }
            return Quaternion.identity;
        }
    }
} 