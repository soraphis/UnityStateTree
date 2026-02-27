namespace UnityStateTree
{
    public interface IStateTreeContext
    {
        public bool TryGetValue<T>(string key, out T value);
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
