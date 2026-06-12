using NUnit.Framework;

namespace UnityStateTree.Test
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
                rootState = new ActionState
                {
                    name = "RootState",
                    depth = 0,
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
                rootState = new ActionState
                {
                    name = "RootState",
                    depth = 0,
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
                rootState = new ActionState
                {
                    name = "RootState",
                    depth = 0,
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
                rootState = new SelectInOrder { name = "Root", depth = 0 }
                    .WithChild(new ActionState
                    {
                        name = "FirstChild",
                        entryConditions = { new MockFalseCondition() }
                    })
                    .WithChild(new ActionState
                    {
                        name = "SecondChild",
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
                rootState = new SelectInOrder { name = "Root", depth = 0 }
                    .WithChild(new ActionState
                    {
                        name = "FirstChild",
                        entryConditions = { new MockTrueCondition() }
                    })
                    .WithChild(new ActionState
                    {
                        name = "SecondChild",
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
                rootState = new SelectInOrder { name = "Root", depth = 0 }
                    .WithChild(new SelectInOrder { name = "Parent" }
                        .WithChild(new ActionState
                        {
                            name = "DeepChild",
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
                rootState = new SelectInOrder { name = "Root", depth = 0 }
                    .WithChild(new ActionState
                    {
                        name = "WithTarget",
                        entryConditions = { new MockContextCondition("hasTarget", true) }
                    })
                    .WithChild(new ActionState
                    {
                        name = "WithoutTarget",
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
