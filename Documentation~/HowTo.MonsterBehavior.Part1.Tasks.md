# How To: Monster Behavior — Part 1 (Tasks)

Conditions decide **which** state is active.
Tasks decide **what** happens while that state is active.

That separation keeps behavior clean:

- No decision logic hidden in tasks.
- No side effects hidden in conditions.

```csharp
using UnityEngine;
using UnityStateTree;

public class SearchForTargetTask : Task
{
    public override TaskStatus OnTick(IStateTreeContext context)
    {
        Debug.Log("Searching for a target...");
        return TaskStatus.Running;
    }
}
```

> **Note:** `SearchForTargetTask` intentionally returns `TaskStatus.Running` forever — it has no way to
> complete on its own. That means the state will never exit by itself.
> In a real world scenario, sub-states would make the monster idle around or wait/delay a bit.
> This means for us, that an extra transition back to `SearchForTarget` is required on the other states to handle the case of losing the target.

```csharp

public class SetMovementTargetPositionTask : Task
{
    public override TaskStatus OnTick(IStateTreeContext context)
    {
        if (!context.TryGetValue("monster", out IMonster monster)) return TaskStatus.Failure;
        if (!context.TryGetValue("targetPosition", out Vector3 targetPosition)) return TaskStatus.Failure;

        monster.SetMoveTarget(targetPosition);
        return TaskStatus.Running;
    }
}

public class AttackTargetTask : Task
{
    public override TaskStatus OnEnterState(IStateTreeContext context)
    {
        if (!context.TryGetValue("monster", out IMonster monster)) return TaskStatus.Failure;

        monster.StartAttack();
        return TaskStatus.Running;
    }

    public override TaskStatus OnTick(IStateTreeContext context)
    {
        Debug.Log("Attacking target.");
        return TaskStatus.Running;
    }

    public override void OnExitState(IStateTreeContext context)
    {
        if (context.TryGetValue("monster", out IMonster monster))
        {
            monster.EndAttack();
        }
    }
}
```
