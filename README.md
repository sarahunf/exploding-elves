# Exploding Elves

Exploding Elves is a Unity-based simulation/game project featuring autonomous elf entities that move, replicate, and interact in a dynamic environment. The project is structured for extensibility and maintainability, leveraging several classic software design patterns.

## Project Overview
- **Entities:** The main actors are elves, each with their own state and behaviors.
- **Spawning:** Elves are spawned via configurable spawners and can replicate under certain conditions.
- **Interaction:** Elves interact with each other and the environment, including collisions and explosions.
- **UI:** The game features a UI for pausing, restarting, and quitting, following a clear separation of concerns.

## Key Design Patterns

### MVVM (Model-View-ViewModel)
- **Location:** `Assets/Scripts/UI/`
- **Usage:** The UI system is organized using the MVVM pattern:
  - **Model:** `GameStateModel` holds the game state (paused, game over, etc.).
  - **ViewModel:** `GameViewModel` exposes state and actions to the UI, raising events for state changes.
  - **View:** `GameUIView` binds to the ViewModel, updating UI elements and responding to user input.
- **Benefit:** This separation improves testability and maintainability of UI logic.

### Factory Pattern
- **Location:** `Assets/Scripts/Actors/Factory/`
- **Usage:** `ElfFactory` implements `IEntityFactory` to create elf entities of various types, managing object pools for efficiency.
- **Benefit:** Centralizes and abstracts entity creation, supporting scalability and code reuse.

### Command Pattern
- **Location:** `Assets/Scripts/Actors/Movement/Commands/`
- **Usage:** Movement actions are encapsulated as command objects (`MoveCommand` implements `IMovementCommand`). `MovementCommandInvoker` queues, executes, and can undo movement commands.
- **Benefit:** Decouples movement logic from execution, enabling features like undo/redo and flexible command processing.

### State Machine Pattern
- **Location:** `Assets/Scripts/Actors/StateMachine/`
- **Usage:** `ElfStateMachine` manages the state transitions (Spawning, Idle, Replicating, Exploding) for each elf, controlling their behavior and effects.
- **Benefit:** Organizes complex entity behavior into manageable, extensible states.

## Extending the Project
- Add new entity types by extending the factory and state machine.
- Add new UI features by following the MVVM structure.
- Implement new commands for additional actions.
