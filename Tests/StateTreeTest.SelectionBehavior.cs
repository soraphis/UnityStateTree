using NUnit.Framework;
using UnityStateTree;

namespace UnityStateTree.Test
{
    public partial class StateTreeTest
    {
        #region Selection Behavior Tests

        [Test]
        public void SelectInOrder_SelectsFirstValidChild()
        {
            var context = new MockContext();
            var stateTree = new StateTreeObject
            {
                rootState = new SelectInOrder
                {
                    name = "Root",
                    depth = 0,
                }
                    .WithChild(new ActionState
                    {
                        name = "Child",
                    })
            };
            var runner = new StateTreeRunner();

            runner.OnEnable(stateTree, context);

            Assert.AreEqual("Child", runner.CurrentState.name);
        }

        [Test]
        public void SelectInOrder_WithNoChildren_BehavesLikeLeaf()
        {
            var context = new MockContext();
            var stateTree = new StateTreeObject
            {
                rootState = new SelectInOrder
                {
                    name = "Root",
                    depth = 0,
                }
            };
            var runner = new StateTreeRunner();

            runner.OnEnable(stateTree, context);

            Assert.IsNotNull(runner.CurrentState);
            Assert.AreEqual("Root", runner.CurrentState.name);
        }

        [Test]
        public void ActionState_StopsAtCurrentState()
        {
            var context = new MockContext();
            var stateTree = new StateTreeObject
            {
                rootState = new ActionState
                {
                    name = "Root",
                    depth = 0,
                }
            };
            var runner = new StateTreeRunner();

            runner.OnEnable(stateTree, context);

            Assert.AreEqual("Root", runner.CurrentState.name);
        }

        [Test]
        public void TryEvaluate_WithNestedValidBranch_ReturnsTrue()
        {
            var root = new SelectInOrder
            {
                name = "Root",
                depth = 0,
            }
            .WithChild(new SelectInOrder
            {
                name = "Branch",
            }
            .WithChild(new ActionState
            {
                name = "Leaf",
            }));

            Assert.IsTrue(root.TryEvaluate());
        }

        #endregion
    }
}
