using Actors.Movement.Commands;
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
        private MovementCommandInvoker commandInvoker;
        private Vector3 tempPosition;
        private Vector3 tempDirection;
        private Vector3 tempReflection;
        private const float BOUNDARY_MARGIN = 0.5f;
        private float minX, maxX, minZ, maxZ;
        
        private void Awake()
        {
            elf = GetComponent<Elf>();
            commandInvoker = new MovementCommandInvoker();
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
                
                minX += BOUNDARY_MARGIN;
                maxX -= BOUNDARY_MARGIN;
                minZ += BOUNDARY_MARGIN;
                maxZ -= BOUNDARY_MARGIN;
            }
            else
            {
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
            commandInvoker.ProcessCommands();
        }
        
        private void Move()
        {
            var config = elf.GetConfig();
            tempPosition = transform.position;

            tempPosition = config.movementStrategy == null ? CalculateDefaultMovement(config) : CalculateStrategyMovement(config);
            
            tempPosition = ConstrainToBoundaries(tempPosition);
            tempPosition = AdjustGroundHeight(tempPosition);
            
            var targetRotation = config.movementStrategy?.CalculateRotation(tempPosition - transform.position) 
                ?? Quaternion.LookRotation(tempPosition - transform.position);
            
            var moveCommand = new MoveCommand(this, tempPosition, targetRotation);
            commandInvoker.AddCommand(moveCommand);
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
            var result = configSo.movementStrategy.CalculateMovement(transform.position, currentDirection, configSo.moveSpeed, Time.deltaTime);
            currentDirection = result.direction;
            return result.position;
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

        private Vector3 ConstrainToBoundaries(Vector3 position)
        {
            if (position.x < minX || position.x > maxX || position.z < minZ || position.z > maxZ)
            {
                var reflection = Vector3.zero;
                
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

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void SetRotation(Quaternion rotation)
        {
            transform.rotation = rotation;
        }
    }
} 