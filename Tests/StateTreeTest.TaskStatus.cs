using NUnit.Framework;
using UnityStateTree;

namespace UnityStateTree.Test
{
    public partial class StateTreeTest
    {
        #region Task Status Tests

        [Test]
        public void Task_ReturningSuccess_TriggersStateCompletion()
        {
            var context = new MockContext();
            var mockTask = new MockTask { StatusToReturn = TaskStatus.Success };
            var stateTree = new StateTreeObject
            {
                rootState = new StateEntry
                {
                    name = "Root",
                    depth = 0,
                    selectionBehavior = SelectionBehavior.None,
                    tasks = { mockTask }
                }
            };
            var runner = new StateTreeRunner();

            runner.OnEnable(stateTree, context);

            Assert.AreEqual(1, mockTask.EnterCount);
        }

        [Test]
        public void Task_ReturningFailure_TriggersStateFailed()
        {
            var context = new MockContext();
            var mockTask = new MockTask { StatusToReturn = TaskStatus.Failure };
            var stateTree = new StateTreeObject
            {
                rootState = new StateEntry
                {
                    name = "Root",
                    depth = 0,
                    selectionBehavior = SelectionBehavior.None,
                    tasks = { mockTask }
                }
            };
            var runner = new StateTreeRunner();

            runner.OnEnable(stateTree, context);

            Assert.AreEqual(1, mockTask.EnterCount);
        }

        [Test]
        public void Task_ReturningRunning_ContinuesNormally()
        {
            var context = new MockContext();
            var mockTask = new MockTask { StatusToReturn = TaskStatus.Running };
            var stateTree = new StateTreeObject
            {
                rootState = new StateEntry
                {
                    name = "Root",
                    depth = 0,
                    selectionBehavior = SelectionBehavior.None,
                    tasks = { mockTask }
                }
            };
            var runner = new StateTreeRunner();

            runner.OnEnable(stateTree, context);
            runner.Update();

            Assert.AreEqual(1, mockTask.EnterCount);
            Assert.AreEqual(1, mockTask.TickCount);
            Assert.AreEqual(0, mockTask.ExitCount);
        }

        [Test]
        public void Task_CompletingOnTick_QueuesTransition_AndSwitchesStateOnNextUpdate()
        {
            var context = new MockContext();
            context.SetValue("completed", false);

            var completingTask = new CompleteAndSetFlagOnFirstTickTask("completed");
            var fallbackTask = new CountingTask();

            var stateTree = new StateTreeObject
            {
                rootState = new StateEntry { name = "Root", depth = 0, selectionBehavior = SelectionBehavior.SelectChildrenInOrder }
                    .WithChild(new StateEntry
                    {
                        name = "Work",
                        selectionBehavior = SelectionBehavior.None,
                        entryConditions = { new MockContextCondition("completed", false) },
                        tasks = { completingTask },
                        transitions =
                        {
                            new TransitionSimple
                            {
                                trigger = TransitionTrigger.OnStateCompleted,
                                targetType = TransitionSimple.TransitionTargetType.ToRoot
                            }
                        }
                    })
                    .WithChild(new StateEntry
                    {
                        name = "Done",
                        selectionBehavior = SelectionBehavior.None,
                        entryConditions = { new MockContextCondition("completed", true) },
                        tasks = { fallbackTask }
                    })
            };

            var runner = new StateTreeRunner();
            runner.OnEnable(stateTree, context);

            runner.Update();
            Assert.AreEqual("Work", runner.CurrentState.name);
            Assert.AreEqual(1, completingTask.TickCount);

            runner.Update();
            Assert.AreEqual("Done", runner.CurrentState.name);
            Assert.AreEqual(1, fallbackTask.EnterCount);

            runner.Update();
            Assert.AreEqual(1, fallbackTask.TickCount);
            Assert.AreEqual(1, completingTask.TickCount);
        }

        [Test]
        public void Task_FailingOnTick_QueuesFailedTransition_AndSwitchesStateOnNextUpdate()
        {
            var context = new MockContext();
            context.SetValue("failed", false);

            var failingTask = new MockTask { StatusToReturn = TaskStatus.Failure };
            var failedStateTask = new CountingTask();

            var stateTree = new StateTreeObject
            {
                rootState = new StateEntry { name = "Root", depth = 0, selectionBehavior = SelectionBehavior.SelectChildrenInOrder }
                    .WithChild(new StateEntry
                    {
                        name = "Work",
                        selectionBehavior = SelectionBehavior.None,
                        entryConditions = { new MockContextCondition("failed", false) },
                        tasks = { failingTask },
                        transitions =
                        {
                            new TransitionSimple
                            {
                                trigger = TransitionTrigger.OnStateFailed,
                                targetType = TransitionSimple.TransitionTargetType.ToRoot
                            }
                        }
                    })
                    .WithChild(new StateEntry
                    {
                        name = "Failed",
                        selectionBehavior = SelectionBehavior.None,
                        entryConditions = { new MockContextCondition("failed", true) },
                        tasks = { failedStateTask }
                    })
            };

            var runner = new StateTreeRunner();
            runner.OnEnable(stateTree, context);

            context.SetValue("failed", true);
            runner.Update();
            Assert.AreEqual("Work", runner.CurrentState.name);

            runner.Update();
            Assert.AreEqual("Failed", runner.CurrentState.name);
            Assert.AreEqual(1, failedStateTask.EnterCount);
        }

        #endregion
    }
}
