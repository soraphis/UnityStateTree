using NUnit.Framework;
using StateTree;

namespace StateTree.Test
{
    public partial class StateTreeTest
    {
        #region State Selection Tests

        [Test]
        public void SingleState_WithNoCondition_IsSelectedCorrectly()
        {
            var context = new MockContext();
            var stateTree = new StateTreeObject
            {
                rootState = new StateEntry
                {
                    name = "RootState",
                    depth = 0,
                    selectionBehavior = SelectionBehavior.None
                }
            };
            var runner = new StateTreeRunner();

            runner.OnEnable(stateTree, context);

            Assert.IsNotNull(runner.CurrentState);
            Assert.AreEqual("RootState", runner.CurrentState.name);
        }

        [Test]
        public void SingleState_WithTrueCondition_IsSelectedCorrectly()
        {
            var context = new MockContext();
            var stateTree = new StateTreeObject
            {
                rootState = new StateEntry
                {
                    name = "RootState",
                    depth = 0,
                    selectionBehavior = SelectionBehavior.None,
                    entryConditions = { new MockTrueCondition() }
                }
            };
            var runner = new StateTreeRunner();

            runner.OnEnable(stateTree, context);

            Assert.IsNotNull(runner.CurrentState);
            Assert.AreEqual("RootState", runner.CurrentState.name);
        }

        [Test]
        public void SingleState_WithFalseCondition_IsNotSelected()
        {
            var context = new MockContext();
            var stateTree = new StateTreeObject
            {
                rootState = new StateEntry
                {
                    name = "RootState",
                    depth = 0,
                    selectionBehavior = SelectionBehavior.None,
                    entryConditions = { new MockFalseCondition() }
                }
            };
            var runner = new StateTreeRunner();

            runner.OnEnable(stateTree, context);

            Assert.IsNull(runner.CurrentState);
        }

        [Test]
        public void TwoStates_FirstConditionFalse_SecondIsSelected()
        {
            var context = new MockContext();
            var stateTree = new StateTreeObject
            {
                rootState = new StateEntry { name = "Root", depth = 0 }
                    .WithChild(new StateEntry
                    {
                        name = "FirstChild",
                        selectionBehavior = SelectionBehavior.None,
                        entryConditions = { new MockFalseCondition() }
                    })
                    .WithChild(new StateEntry
                    {
                        name = "SecondChild",
                        selectionBehavior = SelectionBehavior.None,
                        entryConditions = { new MockTrueCondition() }
                    })
            };
            var runner = new StateTreeRunner();

            runner.OnEnable(stateTree, context);

            Assert.IsNotNull(runner.CurrentState);
            Assert.AreEqual("SecondChild", runner.CurrentState.name);
        }

        [Test]
        public void TwoStates_BothConditionsTrue_FirstIsSelected()
        {
            var context = new MockContext();
            var stateTree = new StateTreeObject
            {
                rootState = new StateEntry { name = "Root", depth = 0 }
                    .WithChild(new StateEntry
                    {
                        name = "FirstChild",
                        selectionBehavior = SelectionBehavior.None,
                        entryConditions = { new MockTrueCondition() }
                    })
                    .WithChild(new StateEntry
                    {
                        name = "SecondChild",
                        selectionBehavior = SelectionBehavior.None,
                        entryConditions = { new MockTrueCondition() }
                    })
            };
            var runner = new StateTreeRunner();

            runner.OnEnable(stateTree, context);

            Assert.IsNotNull(runner.CurrentState);
            Assert.AreEqual("FirstChild", runner.CurrentState.name);
        }

        [Test]
        public void NestedStates_SelectsDeepestValidState()
        {
            var context = new MockContext();
            var stateTree = new StateTreeObject
            {
                rootState = new StateEntry { name = "Root", depth = 0 }
                    .WithChild(new StateEntry { name = "Parent" }
                        .WithChild(new StateEntry
                        {
                            name = "DeepChild",
                            selectionBehavior = SelectionBehavior.None
                        })
                    )
            };
            var runner = new StateTreeRunner();

            runner.OnEnable(stateTree, context);

            Assert.IsNotNull(runner.CurrentState);
            Assert.AreEqual("DeepChild", runner.CurrentState.name);
        }

        [Test]
        public void ContextCondition_CorrectlyEvaluatesContextValue()
        {
            var context = new MockContext();
            context.SetValue("hasTarget", true);
            var stateTree = new StateTreeObject
            {
                rootState = new StateEntry { name = "Root", depth = 0 }
                    .WithChild(new StateEntry
                    {
                        name = "WithTarget",
                        selectionBehavior = SelectionBehavior.None,
                        entryConditions = { new MockContextCondition("hasTarget", true) }
                    })
                    .WithChild(new StateEntry
                    {
                        name = "WithoutTarget",
                        selectionBehavior = SelectionBehavior.None
                    })
            };
            var runner = new StateTreeRunner();

            runner.OnEnable(stateTree, context);

            Assert.IsNotNull(runner.CurrentState);
            Assert.AreEqual("WithTarget", runner.CurrentState.name);
        }

        #endregion
    }
}
