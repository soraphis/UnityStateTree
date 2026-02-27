global using System;
global using UnityEngine;
global using System.Collections.Generic;

public namespace StateTree{
    public class StateTreeAgent : MonoBehaviour
    {
        public StateTreeObject stateTree;
        public IStateTreeContext context;

        private StateTreeRunner runner = new();
        
        void OnEnable()
        {
            if(this.DisableSelfOnTrue(stateTree == null)) return;
            runner.OnEnable(stateTree, context);
            if (this.DisableSelfOnTrue(runner.CurrentState == null)) return;
        }

        private void Update()
        {
            runner.Update();
        }
    }
}