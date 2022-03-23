using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Engine
{
    public static class CollectionUtil
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveSwapBack<T>(this IList<T> items, int index)
        {
            items[index] = items[items.Count - 1];
            items.RemoveAt(items.Count - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveSwapBack<T>(this IList<T> items, T item)
        {
            var index = items.IndexOf(item);
            items.RemoveSwapBack(index);
        }
    }
}
