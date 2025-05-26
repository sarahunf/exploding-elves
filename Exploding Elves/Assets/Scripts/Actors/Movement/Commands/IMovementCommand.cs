using UnityEngine;

namespace Actors.Movement.Commands
{
    public interface IMovementCommand
    {
        void Execute();
        void Undo();
    }
} 