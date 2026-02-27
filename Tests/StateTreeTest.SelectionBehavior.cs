using NUnit.Framework;
using StateTree;

namespace StateTree.Test
{
    public partial class StateTreeTest
    {
        #region Selection Behavior Tests

        [Test]
        public void SelectionBehavior_Default_IsSelectChildrenInOrder()
        {
            var state = new StateEntry();

            Assert.AreEqual(SelectionBehavior.SelectChildrenInOrder, state.selectionBehavior);
        }

        [Test]
        public void SelectionBehaviorNone_StopsAtCurrentState()
        {
            var context = new MockContext();
            var stateTree = new StateTreeObject
            {
                rootState = new StateEntry
                {
                    name = "Root",
                    depth = 0,
                    selectionBehavior = SelectionBehavior.None
                }
                    .WithChild(new StateEntry
                    {
                        name = "ShouldNotBeSelected",
                        selectionBehavior = SelectionBehavior.None
                    })
            };
            var runner = new StateTreeRunner();

            runner.OnEnable(stateTree, context);

            Assert.AreEqual("Root", runner.CurrentState.name);
        }

        [Test]
        public void SelectionBehaviorSelectChildrenInOrder_SelectsFirstValidChild()
        {
            var context = new MockContext();
            var stateTree = new StateTreeObject
            {
                rootState = new StateEntry
                {
                    name = "Root",
                    depth = 0,
                    selectionBehavior = SelectionBehavior.SelectChildrenInOrder
                }
                    .WithChild(new StateEntry
                    {
                        name = "Child",
                        selectionBehavior = SelectionBehavior.None
                    })
            };
            var runner = new StateTreeRunner();

            runner.OnEnable(stateTree, context);

            Assert.AreEqual("Child", runner.CurrentState.name);
        }

        [Test]
        public void SelectionBehaviorSelectChildrenInOrder_WithNoChildren_BehavesLikeNone()
        {
            var context = new MockContext();
            var stateTree = new StateTreeObject
            {
                rootState = new StateEntry
                {
                    name = "Root",
                    depth = 0,
                    selectionBehavior = SelectionBehavior.SelectChildrenInOrder
                }
            };
            var runner = new StateTreeRunner();

            runner.OnEnable(stateTree, context);

            Assert.IsNotNull(runner.CurrentState);
            Assert.AreEqual("Root", runner.CurrentState.name);
        }

        #endregion
    }
}
