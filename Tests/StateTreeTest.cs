using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace UnityStateTree.Test{

    public partial class StateTreeTest
    {
        #region Mock Classes
        
        // Mock Context for testing
        private class MockContext : IStateTreeContext
        {
            private Dictionary<string, object> values = new();

            public void SetValue<T>(string key, T value)
            {
                values[key] = value;
            }

            public bool TryGetValue<T>(string key, out T value)
            {
                if (values.TryGetValue(key, out var obj) && obj is T typedValue)
                {
                    value = typedValue;
                    return true;
                }
                value = default;
                return false;
            }
        }

        // Mock Condition that always returns true
        private class MockTrueCondition : Condition
        {
            public override bool Evaluate(IStateTreeContext context)
            {
                return true;
            }
        }

        // Mock Condition that always returns false
        private class MockFalseCondition : Condition
        {
            public override bool Evaluate(IStateTreeContext context)
            {
                return false;
            }
        }

        // Mock Condition that checks a context value
        private class MockContextCondition : Condition
        {
            public string key;
            public bool expectedValue;

            public MockContextCondition(string key, bool expectedValue)
            {
                this.key = key;
                this.expectedValue = expectedValue;
            }

            public override bool Evaluate(IStateTreeContext context)
            {
                return context.TryGetValue(key, out bool value) && value == expectedValue;
            }
        }

        // Mock Task that tracks lifecycle calls
        private class MockTask : Task
        {
            public int EnterCount { get; private set; }
            public int TickCount { get; private set; }
            public int ExitCount { get; private set; }
            public TaskStatus StatusToReturn { get; set; } = TaskStatus.Running;

            public override TaskStatus OnEnterState(IStateTreeContext context)
            {
                EnterCount++;
                return StatusToReturn;
            }

            public override TaskStatus OnTick(IStateTreeContext context)
            {
                TickCount++;
                return StatusToReturn;
            }

            public override void OnExitState(IStateTreeContext context)
            {
                ExitCount++;
            }
        }

        private class CompleteAndSetFlagOnFirstTickTask : Task
        {
            private readonly string flagKey;
            private bool completed;
            public int TickCount { get; private set; }

            public CompleteAndSetFlagOnFirstTickTask(string flagKey)
            {
                this.flagKey = flagKey;
            }

            public override TaskStatus OnTick(IStateTreeContext context)
            {
                TickCount++;
                if (completed) return TaskStatus.Running;
                completed = true;
                if (context is MockContext mockContext)
                {
                    mockContext.SetValue(flagKey, true);
                }
                return TaskStatus.Success;
            }
        }

        private class CountingTask : Task
        {
            public int EnterCount { get; private set; }
            public int TickCount { get; private set; }

            public override TaskStatus OnEnterState(IStateTreeContext context)
            {
                EnterCount++;
                return TaskStatus.Running;
            }

            public override TaskStatus OnTick(IStateTreeContext context)
            {
                TickCount++;
                return TaskStatus.Running;
            }
        }

        #endregion
    }
}