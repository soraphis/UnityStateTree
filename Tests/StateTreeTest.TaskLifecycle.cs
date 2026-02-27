using NUnit.Framework;
using StateTree;

namespace StateTree.Test
{
    public partial class StateTreeTest
    {
        #region Task Lifecycle Tests

        [Test]
        public void Task_OnEnterState_IsCalledOnce()
        {
            var context = new MockContext();
            var mockTask = new MockTask();
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
            Assert.AreEqual(0, mockTask.TickCount);
            Assert.AreEqual(0, mockTask.ExitCount);
        }

        [Test]
        public void Task_OnTick_IsCalledOnUpdate()
        {
            var context = new MockContext();
            var mockTask = new MockTask();
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
            runner.Update();

            Assert.AreEqual(1, mockTask.EnterCount);
            Assert.AreEqual(2, mockTask.TickCount);
            Assert.AreEqual(0, mockTask.ExitCount);
        }

        [Test]
        public void MultipleTasks_AllCalledInOrder()
        {
            var context = new MockContext();
            var task1 = new MockTask();
            var task2 = new MockTask();
            var task3 = new MockTask();
            var stateTree = new StateTreeObject
            {
                rootState = new StateEntry
                {
                    name = "Root",
                    depth = 0,
                    selectionBehavior = SelectionBehavior.None,
                    tasks = { task1, task2, task3 }
                }
            };
            var runner = new StateTreeRunner();

            runner.OnEnable(stateTree, context);

            Assert.AreEqual(1, task1.EnterCount);
            Assert.AreEqual(1, task2.EnterCount);
            Assert.AreEqual(1, task3.EnterCount);
        }

        #endregion
    }
}
