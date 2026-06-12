namespace UnityStateTree
{
    public interface IStateTreeContext
    {
        public bool TryGetValue<T>(string key, out T value);
        
        void SetValue<T>(string key, T value);
    }

    public interface IStateTreeContext<out TContext> : IStateTreeContext
    {
        TContext TypedContext { get; }
    }

    public struct StateTreeContextSelector<T>
    {
        public string key;
        public bool TrySelect(IStateTreeContext context, out T value)
        {
            return context.TryGetValue(key, out value);
        }
    }
}
