using UnityEngine;
using UnityEngine.Pool;

namespace UnityStateTree.Internal{
    internal static class MonoBehaviorExtensions
    {
        public static bool DisableSelfOnTrue(this MonoBehaviour self, bool condition)
        {
            if(condition) self.enabled = false;
            return condition;
        }
    }
}
