using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Engine
{
    /// <summary>
    /// Utility class providing useful operations for data collections.
    /// </summary>
    public static class CollectionUtil
    {
        /// <summary>
        /// Removes an item at the given <b>index</b> by replacing it with the last element in the collection.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveSwapBack<T>(this IList<T> items, int index)
        {
            items[index] = items[items.Count - 1];
            items.RemoveAt(items.Count - 1);
        }

        /// <summary>
        /// Removes the given <b>item</b> by replacing it with the last element in the collection.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveSwapBack<T>(this IList<T> items, T item)
        {
            var index = items.IndexOf(item);
            items.RemoveSwapBack(index);
        }
    }
}
