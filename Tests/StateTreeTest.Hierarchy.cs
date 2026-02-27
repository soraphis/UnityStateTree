using NUnit.Framework;
using StateTree;

namespace StateTree.Test
{
    public partial class StateTreeTest
    {
        #region Complex Hierarchy Tests

        [Test]
        public void ComplexHierarchy_SelectsCorrectDeepState()
        {
            var context = new MockContext();
            context.SetValue("level1", true);
            context.SetValue("level2", true);

            var stateTree = new StateTreeObject
            {
                rootState = new StateEntry { name = "Root", depth = 0 }
                    .WithChild(new StateEntry
                    {
                        name = "Branch1",
                        entryConditions = { new MockContextCondition("level1", true) }
                    }
                        .WithChild(new StateEntry
                        {
                            name = "Branch1_Child1",
                            selectionBehavior = SelectionBehavior.None,
                            entryConditions = { new MockContextCondition("level2", true) }
                        })
                        .WithChild(new StateEntry
                        {
                            name = "Branch1_Child2",
                            selectionBehavior = SelectionBehavior.None
                        })
                    )
                    .WithChild(new StateEntry
                    {
                        name = "Branch2",
                        selectionBehavior = SelectionBehavior.None
                    })
            };
            var runner = new StateTreeRunner();

            runner.OnEnable(stateTree, context);

            Assert.IsNotNull(runner.CurrentState);
            Assert.AreEqual("Branch1_Child1", runner.CurrentState.name);
        }

        [Test]
        public void StateWithMultipleConditions_AllMustBeTrue()
        {
            var context = new MockContext();
            context.SetValue("condition1", true);
            context.SetValue("condition2", true);

            var stateTree = new StateTreeObject
            {
                rootState = new StateEntry { name = "Root", depth = 0 }
                    .WithChild(new StateEntry
                    {
                        name = "RequiresBoth",
                        selectionBehavior = SelectionBehavior.None,
                        entryConditions =
                        {
                            new MockContextCondition("condition1", true),
                            new MockContextCondition("condition2", true)
                        }
                    })
            };
            var runner = new StateTreeRunner();

            runner.OnEnable(stateTree, context);

            Assert.IsNotNull(runner.CurrentState);
            Assert.AreEqual("RequiresBoth", runner.CurrentState.name);
        }

        [Test]
        public void StateWithMultipleConditions_OneFalse_StateNotSelected()
        {
            var context = new MockContext();
            context.SetValue("condition1", true);
            context.SetValue("condition2", false);

            var stateTree = new StateTreeObject
            {
                rootState = new StateEntry { name = "Root", depth = 0 }
                    .WithChild(new StateEntry
                    {
                        name = "RequiresBoth",
                        selectionBehavior = SelectionBehavior.None,
                        entryConditions =
                        {
                            new MockContextCondition("condition1", true),
                            new MockContextCondition("condition2", true)
                        }
                    })
            };
            var runner = new StateTreeRunner();

            runner.OnEnable(stateTree, context);

            Assert.IsNull(runner.CurrentState);
        }

        #endregion

        #region Parent-Child Relationship Tests

        [Test]
        public void WithChild_SetsParentCorrectly()
        {
            var parent = new StateEntry { name = "Parent", depth = 0 };
            var child = new StateEntry { name = "Child" };

            parent.WithChild(child);

            Assert.AreEqual(parent, child.parent);
        }

        [Test]
        public void WithChild_SetsDepthCorrectly()
        {
            var parent = new StateEntry { name = "Parent", depth = 0 };
            var child = new StateEntry { name = "Child" };

            parent.WithChild(child);

            Assert.AreEqual(1, child.depth);
        }

        [Test]
        public void WithChild_NestedDepthCalculatedCorrectly()
        {
            var root = new StateEntry { name = "Root", depth = 0 };
            var level1 = new StateEntry { name = "Level1" };
            var level2 = new StateEntry { name = "Level2" };

            root.WithChild(level1);
            level1.WithChild(level2);

            Assert.AreEqual(0, root.depth);
            Assert.AreEqual(1, level1.depth);
            Assert.AreEqual(2, level2.depth);
        }

        #endregion
    }
}
