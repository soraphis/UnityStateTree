# Getting Started

This package provides a lightweight StateTree system inspired by behavior-tree style selection with state-machine style execution.

## Core Concepts

- **StateTreeObject**: The root asset/container for your tree.
- **StateEntry**: A node in the tree. A state can have children, entry conditions, tasks, and transitions.
- **Condition**: Decides whether a state (or transition) is valid in the current context.
- **Task**: Runs logic when entering, ticking, and exiting a state.
- **Transition**: Describes how and when to move to another state.
- **StateTreeRunner**: Runtime executor that selects, enters, updates, and exits states.
- **IStateTreeContext**: Your game-specific data interface (`TryGetValue`) used by conditions/tasks.

## Basic Flow

1. Build a tree starting from `StateTreeObject.rootState`.
2. Add child states with `WithChild(...)`.
3. Add `Condition` implementations to control selection.
4. Add `Task` implementations to run behavior.
5. Initialize `StateTreeRunner` with a context.
6. Call `Update()` each frame/tick.

## How Selection Works

- A state is only selectable if all its entry conditions pass.
- `SelectionBehavior.None` means: select this state directly.
- `SelectionBehavior.SelectChildrenInOrder` means: try children from first to last and select the first valid descendant.

## How Execution Works

- On startup, the runner selects a state and enters it.
- Entering a state calls all state tasks via `OnEnterState(...)`.
- During update, active state tasks receive `OnTick(...)`.
- When changing states, `OnExitState(...)` is called while walking up to the common ancestor.

## Minimal Setup Checklist

- Implement your own context class using `IStateTreeContext`.
- Create custom `Condition` types for decision logic.
- Create custom `Task` types for runtime behavior.
- Construct a tree with sensible parent/child hierarchy and selection behavior.
- Start a `StateTreeRunner` and tick it from your game loop (or a MonoBehaviour wrapper).

## Recommended Next Read

Use the tests as executable documentation:

- state selection patterns
- task lifecycle behavior
- null-safety expectations
- hierarchy and parent/child depth behavior
