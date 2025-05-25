using Actors.Movement;
using Config;
using UnityEngine;

namespace Actors
{
    [RequireComponent(typeof(Elf))]
    public class MovementComponent : MonoBehaviour
    {
        private Elf elf;
        private Vector3 currentDirection;
        private float nextDirectionChangeTime;
        private Vector3 lastPosition;
        
        // Boundary constraints
        private const float BOUNDARY_MARGIN = 0.5f;
        private float minX, maxX, minZ, maxZ;
        
        private void Awake()
        {
            elf = GetComponent<Elf>();
            InitializeBoundaries();
        }

        private void InitializeBoundaries()
        {
            var rockWalls = GameObject.FindGameObjectsWithTag("Rock");
            if (rockWalls.Length > 0)
            {
                minX = float.MaxValue;
                maxX = float.MinValue;
                minZ = float.MaxValue;
                maxZ = float.MinValue;

                foreach (var wall in rockWalls)
                {
                    var bounds = wall.GetComponent<Collider>().bounds;
                    minX = Mathf.Min(minX, bounds.min.x);
                    maxX = Mathf.Max(maxX, bounds.max.x);
                    minZ = Mathf.Min(minZ, bounds.min.z);
                    maxZ = Mathf.Max(maxZ, bounds.max.z);
                }

                // Add margin to boundaries
                minX += BOUNDARY_MARGIN;
                maxX -= BOUNDARY_MARGIN;
                minZ += BOUNDARY_MARGIN;
                maxZ -= BOUNDARY_MARGIN;
            }
            else
            {
                // Fallback boundaries if no rocks found
                minX = -50f;
                maxX = 50f;
                minZ = -50f;
                maxZ = 50f;
            }
        }
        
        private void Update()
        {
            Move();
            UpdateWalkingState();
        }
        
        private void Move()
        {
            var config = elf.GetConfig();
            Vector3 newPosition;

            if (config.movementStrategy == null)
            {
                newPosition = CalculateDefaultMovement(config);
            }
            else
            {
                newPosition = CalculateStrategyMovement(config);
            }
            
            // Apply boundary constraints and ground height
            newPosition = ConstrainToBoundaries(newPosition);
            newPosition = AdjustGroundHeight(newPosition);
            
            // Update position and rotation
            UpdatePositionAndRotation(newPosition, config.movementStrategy);
        }

        private Vector3 CalculateDefaultMovement(ElfConfigSO configSo)
        {
            if (currentDirection == Vector3.zero || Time.time >= nextDirectionChangeTime)
            {
                currentDirection = new Vector3(
                    Random.Range(-1f, 1f),
                    0,
                    Random.Range(-1f, 1f)
                ).normalized;
                
                nextDirectionChangeTime = Time.time + configSo.randomDirectionChangeInterval;
            }

            return transform.position + currentDirection * (configSo.moveSpeed * Time.deltaTime);
        }

        private Vector3 CalculateStrategyMovement(ElfConfigSO configSo)
        {
            if (Time.time >= nextDirectionChangeTime)
            {
                currentDirection = configSo.movementStrategy.CalculateDirection(currentDirection, Time.deltaTime);
                nextDirectionChangeTime = Time.time + configSo.movementStrategy.directionChangeInterval;
            }

            return configSo.movementStrategy.CalculateMovement(transform.position, currentDirection, configSo.moveSpeed, Time.deltaTime);
        }

        private Vector3 AdjustGroundHeight(Vector3 position)
        {
            if (Physics.Raycast(position + Vector3.up * 0.1f, Vector3.down, out var hit, 1f))
            {
                float targetHeight = hit.point.y + 0.1f;
                position.y = Mathf.Abs(targetHeight - transform.position.y) <= 0.5f ? targetHeight : transform.position.y;
            }
            else
            {
                position.y = transform.position.y;
            }
            return position;
        }

        private void UpdatePositionAndRotation(Vector3 newPosition, MovementStrategySO strategy)
        {
            Vector3 movement = newPosition - transform.position;
            if (movement != Vector3.zero)
            {
                Quaternion targetRotation = strategy?.CalculateRotation(movement) ?? Quaternion.LookRotation(movement);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
            transform.position = newPosition;
        }

        private Vector3 ConstrainToBoundaries(Vector3 position)
        {
            if (position.x < minX || position.x > maxX || position.z < minZ || position.z > maxZ)
            {
                Vector3 reflection = Vector3.zero;
                
                if (position.x < minX)
                {
                    reflection.x = 1;
                    position.x = minX;
                }
                else if (position.x > maxX)
                {
                    reflection.x = -1;
                    position.x = maxX;
                }
                
                if (position.z < minZ)
                {
                    reflection.z = 1;
                    position.z = minZ;
                }
                else if (position.z > maxZ)
                {
                    reflection.z = -1;
                    position.z = maxZ;
                }
                
                currentDirection = Vector3.Reflect(currentDirection, reflection.normalized);
                nextDirectionChangeTime = Time.time + 0.1f;
            }
            
            return position;
        }
        
        private void UpdateWalkingState()
        {
            bool isMoving = (transform.position - lastPosition).sqrMagnitude > 0.0001f;
            elf.GetView().SetWalking(isMoving);
            lastPosition = transform.position;
        }
        
        public void SetDirection(Vector3 direction)
        {
            currentDirection = direction;
        }
        
        public Vector3 GetCurrentDirection()
        {
            return currentDirection;
        }
        
        public void SetNextDirectionChangeTime(float time)
        {
            nextDirectionChangeTime = time;
        }
    }
} 