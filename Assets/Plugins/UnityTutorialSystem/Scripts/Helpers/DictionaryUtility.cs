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
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Dictionary<TKey,TValue> AddRange<TKey, TValue>(this Dictionary<TKey,TValue> target, Dictionary<TKey,TValue> source)
        {
            return AddRange<Dictionary<TKey,TValue>, Dictionary<TKey,TValue>, TKey, TValue>(target, source);
        }
        /// <summary>
        ///   Implements the missing AddRange method found in other collection classes.
        ///   This implementation preserves the actual type of the dictionary implementation
        ///   so that we dont end up with boxed or up-casted instances.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <typeparam name="TDictionary"></typeparam>
        /// <typeparam name="TEnumerable"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static TDictionary AddRange<TDictionary, TEnumerable, TKey, TValue>(this TDictionary target, TEnumerable source)
            where TDictionary: IDictionary<TKey,TValue>
            where TEnumerable: IEnumerable<KeyValuePair<TKey, TValue>>
        {
            foreach (var s in source)
            {
                target.Add(s.Key, s.Value);
            }

            return target;
        }
    }
}