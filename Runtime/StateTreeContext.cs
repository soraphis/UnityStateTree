using System.Collections.Generic;

namespace UnityStateTree
{
    /// <summary>
    /// A strongly-typed context wrapper. Pass your own class/struct as TContext
    /// and use Task&lt;TContext&gt; / Condition&lt;TContext&gt; to access it directly.
    /// Still supports the dictionary-based IStateTreeContext for mixed usage.
    /// </summary>
    public class StateTreeContext<TContext> : IStateTreeContext<TContext>
    {
        public TContext TypedContext { get; set; }

        private readonly Dictionary<string, object> values = new();

        public StateTreeContext(TContext typedContext)
        {
            TypedContext = typedContext;
        }

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

