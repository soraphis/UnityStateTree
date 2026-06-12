using System;
using System.Collections.Generic;

namespace UnityStateTree.Internal
{
    internal static class CollectionExtensions
    {
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static bool AnyFast<T>(this List<T> source, Func<T, bool> predicate)
        {
            for (var i = 0; i < source.Count; i++)
            {
                if (predicate(source[i])) return true;
            }
            return false;
        }
    }
}