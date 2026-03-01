# How To: Monster Behavior — Part 2 (Build the Tree)

Now we assemble the tree.

The order of root children is important because we use `SelectChildrenInOrder`:

1. AttackTarget (highest priority)
2. ChaseTarget
3. SearchForTarget (fallback)

This gives us a clean "best valid option wins" flow.

## Complete Transition Matrix

Before looking at the code, it helps to reason about every edge in the graph:

| From | → To | Condition |
|---|---|---|
| `SearchForTarget` | `ChaseTarget` | `HasTarget` |
| `ChaseTarget` | `AttackTarget` | `TargetInAttackRange` |
| `ChaseTarget` | `SearchForTarget` | `!HasTarget` |
| `AttackTarget` | `ChaseTarget` | `HasTarget && !InRange` |
| `AttackTarget` | `SearchForTarget` | `!HasTarget` |

The two `→ SearchForTarget` edges are easy to forget. Without them, a state that returns
`TaskStatus.Running` forever (like the chase or attack tasks do here) will never exit on its own
when the target is lost — the monster gets stuck at the last known position.

Transitions inside a state are checked **in order**. Put the more specific condition first so it wins
over the broader fallback (`!HasTarget`) when both could fire at the same tick.

```csharp
using UnityStateTree;

public static class MonsterBehaviorTreeFactory
{
    public static StateTreeObject Create()
    {
        var root = new StateEntry
        {
            name = "Root",
            depth = 0,
            selectionBehavior = SelectionBehavior.SelectChildrenInOrder
        }
        .WithChild(
            new StateEntry
            {
                name = "AttackTarget",
                selectionBehavior = SelectionBehavior.None,
                entryConditions = { new TargetInAttackRangeCondition() },
                tasks = { new AttackTargetTask() },
                transitions =
                {
                    // Target moved out of range → chase it
                    new TransitionConditionalWithTarget
                    {
                        trigger = TransitionTrigger.OnTick,
                        targetState = "ChaseTarget",
                        conditions =
                        {
                            new HasTargetCondition(),
                            new NotCondition(new TargetInAttackRangeCondition())
                        }
                    },
                    // Target lost entirely → go search
                    new TransitionConditionalWithTarget
                    {
                        trigger = TransitionTrigger.OnTick,
                        targetState = "SearchForTarget",
                        conditions = { new NotCondition(new HasTargetCondition()) }
                    }
                }
            }
        )
        .WithChild(
            new StateEntry
            {
                name = "ChaseTarget",
                selectionBehavior = SelectionBehavior.None,
                entryConditions =
                {
                    new HasTargetCondition(),
                    new NotCondition(new TargetInAttackRangeCondition())
                },
                tasks = { new SetMovementTargetPositionTask() },
                transitions =
                {
                    // Close enough → attack
                    new TransitionConditionalWithTarget
                    {
                        trigger = TransitionTrigger.OnTick,
                        targetState = "AttackTarget",
                        conditions = { new TargetInAttackRangeCondition() }
                    },
                    // Reached last known position but target is gone → go search
                    new TransitionConditionalWithTarget
                    {
                        trigger = TransitionTrigger.OnTick,
                        targetState = "SearchForTarget",
                        conditions = { new NotCondition(new HasTargetCondition()) }
                    }
                }
            }
        )
        .WithChild(
            new StateEntry
            {
                name = "SearchForTarget",
                selectionBehavior = SelectionBehavior.None,
                entryConditions = { new NotCondition(new HasTargetCondition()) },
                tasks = { new SearchForTargetTask() },
                transitions =
                {
                    // Target found → start chasing
                    new TransitionConditionalWithTarget
                    {
                        trigger = TransitionTrigger.OnTick,
                        targetState = "ChaseTarget",
                        conditions = { new HasTargetCondition() }
                    }
                }
            }
        );

        return new StateTreeObject
        {
            rootState = root
        };
    }
}
```

## Minimal Runner Example

```csharp
using UnityEngine;
using UnityStateTree;

public class DemoMonster : IMonster
{
    public void SetMoveTarget(Vector3 position)
    {
        // Hook this into your movement system.
    }

    public void StartAttack()
    {
        // Play attack animation / enable hitboxes.
    }

    public void EndAttack()
    {
        // Stop attack animation / disable hitboxes.
    }
}

public class MonsterBehaviorExample
{
    private readonly StateTreeRunner runner = new();
    private readonly IMonster monster = new DemoMonster();
    private MonsterContext context;

    public void Start()
    {
        context = new MonsterContext(monster);
        var tree = MonsterBehaviorTreeFactory.Create();
        runner.OnEnable(tree, context);
    }

    public void Tick()
    {
        runner.Update();
    }

    // Example: called by your gameplay systems
    public void OnSensedTarget(Vector3 targetPosition, float distance)
    {
        context.SetHasTarget(true);
        context.SetTargetPosition(targetPosition);
        context.SetTargetDistance(distance);
    }

    public void OnLostTarget()
    {
        context.SetHasTarget(false);
        context.SetTargetDistance(float.MaxValue);
    }
}
```