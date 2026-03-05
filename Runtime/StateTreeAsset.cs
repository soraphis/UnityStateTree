using UnityEngine;

namespace UnityStateTree
{
    [CreateAssetMenu(menuName = "StateTree/Asset", fileName = "StateTreeAsset", order = 0)]
    public class StateTreeAsset : ScriptableObject
    {
        public StateTreeObject stateTree;
    }
}
