using UnityEngine;
using UnityStateTree.Internal;

namespace UnityStateTree{
    public class StateTreeAgent : MonoBehaviour
    {
        public StateTreeAsset stateTreeAsset;
        public IStateTreeContext context;

        private StateTreeRunner runner = new();
        
        void OnEnable()
        {
            if(this.DisableSelfOnTrue(stateTreeAsset == null)) return;
            runner.OnEnable(stateTreeAsset.stateTree, context);
            if (this.DisableSelfOnTrue(runner.CurrentState == null)) return;
        }

        private void Update()
        {
            runner.Update();
        }
    }
}