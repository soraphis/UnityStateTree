# How To: Monster Behavior — Part 0 (Context and Conditions)

Before we define behavior, we need shared facts about the world.

For this monster, the world facts are:

- Do we currently have a target?
- If yes, how far away is it?
- What is our attack range?
- What is the target position?
- Which actor should execute actions?

That is enough to drive all three states.

```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityStateTree;

public interface IMonster
{
    void SetMoveTarget(Vector3 position);
    void StartAttack();
    void EndAttack();
}

public class MonsterContext : IStateTreeContext
{
    private readonly Dictionary<string, object> values = new();

    public MonsterContext(IMonster monster)
    {
        values["monster"] = monster;
        values["hasTarget"] = false;
        values["targetDistance"] = float.MaxValue;
        values["targetPosition"] = Vector3.zero;
        values["attackRange"] = 2.0f;
    }

    public bool TryGetValue<T>(string key, out T value)
    {
        if (values.TryGetValue(key, out var raw) && raw is T typed)
        {
            value = typed;
            return true;
        }

        value = default;
        return false;
    }

    public void SetHasTarget(bool hasTarget) => values["hasTarget"] = hasTarget;
    public void SetTargetDistance(float targetDistance) => values["targetDistance"] = targetDistance;
    public void SetTargetPosition(Vector3 targetPosition) => values["targetPosition"] = targetPosition;
    public void SetAttackRange(float attackRange) => values["attackRange"] = attackRange;
}
```

## Conditions

Think of conditions as tiny yes/no business rules.

Keep them small and focused. If each condition answers exactly one question, your tree stays understandable even months later.

With `NotCondition`, you can keep this set minimal and compose more complex logic in the tree definition.

```csharp
using UnityStateTree;

public class HasTargetCondition : Condition
{
    public override bool Evaluate(IStateTreeContext context)
    {
        return context.TryGetValue("hasTarget", out bool hasTarget) && hasTarget;
    }
}

public class TargetInAttackRangeCondition : Condition
{
    public override bool Evaluate(IStateTreeContext context)
    {
        if (!context.TryGetValue("hasTarget", out bool hasTarget) || !hasTarget) return false;
        if (!context.TryGetValue("targetDistance", out float targetDistance)) return false;
        if (!context.TryGetValue("attackRange", out float attackRange)) return false;

        return targetDistance <= attackRange;
    }
}
```

### Composed Conditions (using `NotCondition`)

You can build the previous convenience rules directly where needed:

- `HasNoTarget` -> `new NotCondition(new HasTargetCondition())`
- `HasTargetButOutOfRange` ->
  - `new HasTargetCondition()`
  - `new NotCondition(new TargetInAttackRangeCondition())`

Because state and transition conditions are AND-chained, this composition stays explicit and modular.

Next: Part 1 adds behavior tasks that run once a state has been selected.
