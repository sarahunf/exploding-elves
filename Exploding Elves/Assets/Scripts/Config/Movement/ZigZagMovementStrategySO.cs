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
        
        public override IMovementStrategy.MovementResult CalculateMovement(Vector3 currentPosition, Vector3 currentDirection, float moveSpeed, float deltaTime)
        {
            Elf elf = FindElfAtPosition(currentPosition);
            if (elf == null) 
                return new IMovementStrategy.MovementResult(currentPosition, currentDirection);
            var data = GetInstanceData(elf);
            if (!data.hasInitialized)
            {
                data.baseDirection = new Vector3(
                    Random.Range(-1f, 1f),
                    0,
                    Random.Range(-1f, 1f)
                ).normalized;
                data.currentZigzagAngle = Random.Range(0f, 360f);
                data.hasInitialized = true;
                data.lastSafePosition = currentPosition;
            }
            var currentZigzagDirection = GetZigzagDirection(data);
            bool isNearWall = Physics.Raycast(currentPosition + Vector3.up * 0.1f, currentZigzagDirection, out var hit, raycastDistance, obstacleLayer);
            if (isNearWall)
            {
                data.isZigging = !data.isZigging;
                var hitNormal = hit.normal;
                data.baseDirection = Vector3.Reflect(currentZigzagDirection, hitNormal);
                data.baseDirection.y = 0;
                data.baseDirection.Normalize();
                currentPosition += hitNormal * wallAvoidanceDistance;
            }
            float distanceMoved = Vector3.Distance(currentPosition, data.lastSafePosition);
            if (distanceMoved < 0.01f)
            {
                data.stuckTimer += deltaTime;
                if (data.stuckTimer > 0.5f)
                {
                    var escapeDirection = Vector3.zero;
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = i * 45f * Mathf.Deg2Rad;
                        var testDirection = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                        if (!Physics.Raycast(currentPosition + Vector3.up * 0.1f, testDirection, wallAvoidanceDistance, obstacleLayer))
                        {
                            escapeDirection = testDirection;
                            break;
                        }
                    }
                    if (escapeDirection != Vector3.zero)
                    {
                        currentPosition += escapeDirection * (moveSpeed * deltaTime * positionCorrectionStrength);
                        data.baseDirection = escapeDirection;
                        data.currentZigzagAngle = Mathf.Atan2(escapeDirection.z, escapeDirection.x) * Mathf.Rad2Deg;
                    }
                    else
                    {
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
            var horizontalMovement = currentZigzagDirection * (moveSpeed * deltaTime);
            var newPosition = currentPosition + horizontalMovement;
            if (Physics.Raycast(newPosition + Vector3.up * 0.1f, Vector3.zero, 0.1f, obstacleLayer))
            {
                return new IMovementStrategy.MovementResult(currentPosition, currentZigzagDirection);
            }
            return new IMovementStrategy.MovementResult(newPosition, currentZigzagDirection);
        }
        
        private Elf FindElfAtPosition(Vector3 position)
        {
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
            Elf elf = FindElfAtPosition(Vector3.zero);
            if (elf == null) 
                return currentDirection;
            
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
            return movement != Vector3.zero ? Quaternion.LookRotation(movement) : Quaternion.identity;
        }
    }
} 