using UnityEngine;

namespace Actors.Movement.Commands
{
    public class MoveCommand : IMovementCommand
    {
        private readonly MovementComponent movementComponent;
        private readonly Vector3 targetPosition;
        private readonly Vector3 previousPosition;
        private readonly Quaternion targetRotation;
        private readonly Quaternion previousRotation;

        public MoveCommand(MovementComponent movementComponent, Vector3 targetPosition, Quaternion targetRotation)
        {
            this.movementComponent = movementComponent;
            this.targetPosition = targetPosition;
            this.previousPosition = movementComponent.transform.position;
            this.targetRotation = targetRotation;
            this.previousRotation = movementComponent.transform.rotation;
        }

        public void Execute()
        {
            movementComponent.transform.position = targetPosition;
            movementComponent.transform.rotation = targetRotation;
        }

        public void Undo()
        {
            movementComponent.transform.position = previousPosition;
            movementComponent.transform.rotation = previousRotation;
        }
    }
} 