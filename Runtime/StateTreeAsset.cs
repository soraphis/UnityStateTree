using System;
using UnityEngine;

namespace UnityStateTree
{
    [CreateAssetMenu(menuName = "StateTree/Asset", fileName = "StateTreeAsset", order = 0)]
    public class StateTreeAsset : ScriptableObject
    {
        public StateTreeObject stateTree;


        private void OnEnable()
        {
            OnValidate();
        }

        private void OnValidate()
        {
            // ensure there is a root state:
            stateTree ??= new StateTreeObject();

            if (stateTree.rootState == null)
            {
                stateTree.rootState = new SelectInOrder
                {
                    name = "Root",
                    depth = 0,
                };
            }
        }
    }
}
