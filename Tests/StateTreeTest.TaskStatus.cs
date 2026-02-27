using NUnit.Framework;
using StateTree;

namespace StateTree.Test
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

        #endregion
    }
}
