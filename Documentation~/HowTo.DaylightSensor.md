# How To: Daylight Sensor

This tutorial shows how to build a simple StateTree setup that runs a task while your context reports daytime.

## Goal

- Read a `isDaytime` flag from context.
- Run a task every tick while it is daytime.
- Skip daylight behavior when it is nighttime.

## Step 1: Create a Context

Create a context class that implements `IStateTreeContext` and exposes an `isDaytime` value.

```csharp
using System.Collections.Generic;
using UnityStateTree;

public class DaylightContext : IStateTreeContext
{
    private readonly Dictionary<string, object> values = new();

    public DaylightContext(bool isDaytime)
    {
        values["isDaytime"] = isDaytime;
    }

    public void SetDaytime(bool isDaytime)
    {
        values["isDaytime"] = isDaytime;
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
}
```

## Step 2: Create Conditions

Use one condition for daytime state entry, and one inverse condition for leaving that state.

```csharp
using UnityStateTree;

public class IsDaytimeCondition : Condition
{
    public override bool Evaluate(IStateTreeContext context)
    {
        return context.TryGetValue("isDaytime", out bool isDaytime) && isDaytime;
    }
}

public class IsNighttimeCondition : Condition
{
    public override bool Evaluate(IStateTreeContext context)
    {
        return !context.TryGetValue("isDaytime", out bool isDaytime) || !isDaytime;
    }
}
```

## Step 3: Create a Daylight Task

This task runs every update while the Daylight state is active. It does not re-check the state condition.

```csharp
using UnityEngine;
using UnityStateTree;

public class DaylightWorkTask : Task
{
    public override TaskStatus OnTick(IStateTreeContext context)
    {
        Debug.Log("Daylight work is running.");
        return TaskStatus.Running;
    }
}
```

## Step 4: Build the StateTree

Create the tree with a nested fluent style so structure is readable directly in code.

```csharp
using UnityStateTree;

public static class DaylightTreeFactory
{
    public static StateTreeObject Create()
    {
        var root = new StateEntry
        {
            name = "Root",
            depth = 0,
        }
        .WithChild(
            new StateEntry
            {
                name = "DaylightState",
                selectionBehavior = SelectionBehavior.None,
                entryConditions = { new IsDaytimeCondition() },
                tasks = { new DaylightWorkTask() },
                transitions =
                {
                    new TransitionConditionalWithTarget
                    {
                        trigger = TransitionTrigger.OnTick,
                        targetState = "Root",
                        conditions = { new IsNighttimeCondition() }
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

## Step 5: Run It

```csharp
using UnityStateTree;

public class DaylightExample
{
    private readonly StateTreeRunner runner = new();
    private readonly DaylightContext context = new(isDaytime: true);
    private bool isDaytime = true;
    private float elapsedSeconds;

    public void Start()
    {
        var tree = DaylightTreeFactory.Create();
        runner.OnEnable(tree, context);
    }

    public void Tick(float deltaTime)
    {
        elapsedSeconds += deltaTime;
        if (elapsedSeconds >= 5f)
        {
            elapsedSeconds -= 5f;
            isDaytime = !isDaytime;
            context.SetDaytime(isDaytime);
        }

        runner.Update();
    }
}
```
