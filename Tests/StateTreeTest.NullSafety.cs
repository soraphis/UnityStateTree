using System;
using NUnit.Framework;
using StateTree;

namespace StateTree.Test
{
    public partial class StateTreeTest
    {
        #region Null Safety Tests

        [Test]
        public void OnEnable_WithNullStateTree_ThrowsException()
        {
            var context = new MockContext();
            var runner = new StateTreeRunner();

            Assert.Throws<ArgumentNullException>(() => runner.OnEnable(null, context));
        }

        [Test]
        public void OnEnable_WithNullContext_ThrowsException()
        {
            var stateTree = new StateTreeObject
            {
                rootState = new StateEntry { name = "Root", depth = 0 }
            };
            var runner = new StateTreeRunner();

            Assert.Throws<ArgumentNullException>(() => runner.OnEnable(stateTree, null));
        }

        #endregion
    }
}
