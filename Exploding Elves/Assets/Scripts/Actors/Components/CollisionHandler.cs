using System.Collections;
using System.Collections.Generic;
using Actors.Interface;
using UnityEngine;

namespace Actors.Components
{
    public class CollisionHandler : MonoBehaviour
    {
        private static HashSet<(int, int)> processedCollisions = new HashSet<(int, int)>();
        private static float lastCleanupTime = 0f;
        private float collisionCleanupInterval = 5f;
        private Elf owner;

        private void Awake()
        {
            owner = GetComponent<Elf>();
        }

        private void Update()
        {
            if (Time.time - lastCleanupTime > collisionCleanupInterval)
            {
                processedCollisions.Clear();
                lastCleanupTime = Time.time;
            }
        }

        public void HandleCollision(IEntity other)
        {
            if (!owner.CanReplicate()) return;

            var otherElf = other as Elf;
            if (otherElf == null) return;
            
            int id1 = gameObject.GetInstanceID();
            int id2 = otherElf.gameObject.GetInstanceID();
            var collisionId = (Mathf.Min(id1, id2), Mathf.Max(id1, id2));
            
            if (!processedCollisions.Add(collisionId)) return;
            
            if (other.GetEntityType() == owner.GetEntityType())
            {
                owner.HandleReplication();
                otherElf.HandleReplication();
            }
            else
            {
                owner.Explode();
                otherElf.Explode();
            }
            StartCoroutine(RemoveCollisionId(collisionId));
        }

        private IEnumerator RemoveCollisionId((int, int) collisionId)
        {
            yield return new WaitForSeconds(0.1f);
            if (processedCollisions.Contains(collisionId))
            {
                processedCollisions.Remove(collisionId);
            }
        }

        public void HandleRockCollision(Collider rock)
        {
            var movementComponent = GetComponent<MovementComponent>();
            if (movementComponent == null) return;

            var collisionNormal = (transform.position - rock.transform.position).normalized;
            var currentDirection = movementComponent.GetCurrentDirection();
            float yDirection = currentDirection.y;
            var newDirection = Vector3.Reflect(currentDirection, collisionNormal);
            newDirection.y = yDirection;
            movementComponent.SetDirection(newDirection);
            movementComponent.SetNextDirectionChangeTime(Time.time + owner.GetConfig().randomDirectionChangeInterval);
            transform.position += newDirection * 0.1f;
        }
    }
} 