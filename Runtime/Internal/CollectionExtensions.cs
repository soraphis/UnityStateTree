namespace UnityStateTree.Internal
{
    internal static class CollectionExtensions
    {
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool AllFast<T>(this List<T> source, Func<T, bool> predicate)
        {
            for (var i = 0; i < source.Count; i++)
            {
                if (!predicate(source[i])) return false;
            }
            return true;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool AnyFast<T>(this List<T> source, Func<T, bool> predicate)
        {
            for (var i = 0; i < source.Count; i++)
            {
                if (predicate(source[i])) return true;
            }
            return false;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static T FirstOrDefaultFast<T>(this List<T> source, Func<T, bool> predicate) where T : class
        {
            for (var i = 0; i < source.Count; i++)
            {
                var item = source[i];
                if (predicate(item)) return item;
            }
            return null;
        }
    }
}