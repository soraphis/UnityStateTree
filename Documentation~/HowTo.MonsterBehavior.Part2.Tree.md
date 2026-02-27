# How To: Monster Behavior — Part 2 (Build the Tree)

Now we assemble the tree.

The order of root children is important because we use `SelectChildrenInOrder`:

1. AttackTarget (highest priority)
2. ChaseTarget
3. SearchForTarget (fallback)

This gives us a clean “best valid option wins” flow.

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
                    new TransitionConditionalWithTarget
                    {
                        trigger = TransitionTrigger.OnTick,
                        targetState = "ChaseTarget",
                        conditions =
                        {
                            new HasTargetCondition(),
                            new NotCondition(new TargetInAttackRangeCondition())
                        }
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
                    new TransitionConditionalWithTarget
                    {
                        trigger = TransitionTrigger.OnTick,
                        targetState = "AttackTarget",
                        conditions = { new TargetInAttackRangeCondition() }
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
                tasks = { new SearchForTargetTask() }
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
