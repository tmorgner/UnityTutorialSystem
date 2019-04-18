using System.Collections.Generic;

namespace UnityTutorialSystem.Helpers
{
    /// <summary>
    ///   Dictionary helper methods.
    /// </summary>
    public static class DictionaryUtility
    {
        /// <summary>
        ///   Implements the missing AddRange method found in other collection classes.
        ///   This implementation preserves the actual type of the dictionary implementation
        ///   so that we dont end up with boxed or up-casted instances.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <typeparam name="TDictionary"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static TDictionary AddRange<TDictionary, TKey, TValue>(this TDictionary target, ICollection<KeyValuePair<TKey, TValue>> source)
            where TDictionary: IDictionary<TKey,TValue>
        {
            foreach (var s in source)
            {
                target.Add(s.Key, s.Value);
            }

            return target;
        }
    }
}