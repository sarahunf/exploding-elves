using System.Collections.Generic;

namespace Actors.Movement.Commands
{
    public class MovementCommandInvoker
    {
        private readonly Queue<IMovementCommand> commandQueue = new Queue<IMovementCommand>();
        private readonly Stack<IMovementCommand> commandHistory = new Stack<IMovementCommand>();
        private readonly int maxHistorySize = 10;

        public void AddCommand(IMovementCommand command)
        {
            commandQueue.Enqueue(command);
        }

        public void ProcessCommands()
        {
            if (commandQueue.Count > 0)
            {
                var command = commandQueue.Dequeue();
                command.Execute();
                commandHistory.Push(command);
                if (commandHistory.Count > maxHistorySize)
                {
                    commandHistory.Pop();
                }
            }
        }

        public void UndoLastCommand()
        {
            if (commandHistory.Count > 0)
            {
                var command = commandHistory.Pop();
                command.Undo();
            }
        }

        public void ClearCommands()
        {
            commandQueue.Clear();
            commandHistory.Clear();
        }
    }
} 