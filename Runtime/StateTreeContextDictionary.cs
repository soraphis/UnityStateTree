using System.Collections.Generic;
using UnityEngine;

namespace UnityStateTree
{
    
    /// <summary>
    /// A general purpose dictionary that can be used as a context.
    /// be aware of boxing and unboxing as overhead
    /// </summary> 
    [CreateAssetMenu(menuName = "StateTree/Context Dictionary")]
    public class StateTreeContextDictionary : ScriptableObject, IStateTreeContext
    {
        private readonly Dictionary<string, object> values = new();
        
        public bool TryGetValue<T>(string key, out T value)
        {
            if (values.TryGetValue(key, out var raw) && raw is T typed)
            {
                value = typed;
                return true;
            }

            value = default;
            return false;
        }

        public void SetValue<T>(string key, T value)
        {
            values[key] = value;
        }
    }
}