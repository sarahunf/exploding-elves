# Component Pattern

Location: Assets/Scripts/Actors/Components/

## Overview
The Component Pattern is implemented to separate concerns and improve maintainability of the Elf entity. Each component handles a specific aspect of the Elf's behavior, making the code more modular and easier to maintain.

## Components

### CollisionHandler
- Handles all collision-related logic for the Elf
- Manages collision detection and response
- Implements collision deduplication to prevent multiple collision events
- Handles both Elf-to-Elf and Elf-to-Rock collisions
- Provides methods for replication and explosion triggers

### ParticleEffectHandler
- Manages all particle effects for the Elf
- Handles explosion and spawning visual effects
- Integrates with the particle pooling system
- Controls particle system lifecycle and cleanup

### MovementComponent
- Manages the Elf's movement behavior
- Handles direction changes and movement calculations
- Controls movement speed and patterns

## Benefits
1. **Separation of Concerns**: Each component handles a specific aspect of the Elf's behavior
2. **Improved Maintainability**: Changes to one aspect (e.g., collision handling) don't affect others
3. **Better Testability**: Components can be tested in isolation
4. **Code Reusability**: Components can be reused across different entity types
5. **Easier Debugging**: Issues can be isolated to specific components

## Usage
The Elf class acts as the main entity that composes these components:

```csharp
public class Elf : Entity
{
    private MovementComponent movementComponent;
    private CollisionHandler collisionHandler;
    private ParticleEffectHandler particleHandler;

    private void Awake()
    {
        movementComponent = GetComponent<MovementComponent>();
        collisionHandler = GetComponent<CollisionHandler>();
        particleHandler = GetComponent<ParticleEffectHandler>();
    }
}
```

Each component is responsible for its own initialization and cleanup, while the Elf class coordinates their interactions. 