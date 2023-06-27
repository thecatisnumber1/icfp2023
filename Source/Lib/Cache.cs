using System.Collections;

namespace Lib
{
    /// <summary>
    /// A cache of T keys to V values.
    /// Keys can be
    /// 1. A primitive (int, string, etc)
    /// 2. An object that overrides GetHashCode etc properly
    /// 3. An IEnumerable of #1 or #2
    /// </summary>
    public class Cache<T, V>
        where T : notnull
    {
        private readonly Func<T, V> createValue;
        private readonly bool isEnumerable;

        // Normal dictionary
        private readonly Dictionary<T, V>? dict;
        // Special dictionary for IEnumerable T
        private readonly Dictionary<int, V>? enumerableDict;

        /// <summary>
        /// Initializes an empty cache with a function that produces values on cache miss.
        /// </summary>
        /// <param name="createValue">Given a key T, produce a value V. Used to produce a value on cache miss.</param>
        public Cache(Func<T, V> createValue)
        {
            this.createValue = createValue;
            isEnumerable = Cache.IsIEnumerable(typeof(T));

            // Initialize the correct dictionary
            dict = isEnumerable ? null : new Dictionary<T, V>();
            enumerableDict = isEnumerable ? new Dictionary<int, V>() : null;
        }

        /// <summary>
        /// Return the value produced by createValue(param), either from cache or by calling the function.
        /// </summary>
        public V Get(T param)
        {
            if (isEnumerable && enumerableDict != null)
            {
                int hashCode = Cache.HashList((IEnumerable)param);
                if (!enumerableDict.ContainsKey(hashCode))
                {
                    enumerableDict[hashCode] = createValue(param);
                }
                return enumerableDict[hashCode];
            }
            else if (!isEnumerable && dict != null)
            {
                if (!dict.ContainsKey(param))
                {
                    dict[param] = createValue(param);
                }

                return dict[param];
            }

            // Should never get here
            throw new Exception("Cache initialization failure");
        }

        /// <summary>
        /// Clear all entries in the cache
        /// </summary>
        public void Clear()
        {
            dict?.Clear();
            enumerableDict?.Clear();
        }
    }

    /// <summary>
    /// A cache of (T1, T2) keys to V values.
    /// Keys can be
    /// 1. A primitive (int, string, etc)
    /// 2. An object that overrides GetHashCode etc properly
    /// 3. An IEnumerable of #1 or #2
    /// For best performance, T1 should have the larger spread. eg. T1 = 10^9 options, T2 = 10^6 options.
    /// </summary>
    public class Cache<T1, T2, V>
        where T1 : notnull
        where T2 : notnull
    {
        private readonly Cache<T2, Cache<T1, V>> cache;

        /// <summary>
        /// Initializes an empty cache with a function that produces values on cache miss.
        /// </summary>
        /// <param name="createValue">Given keys T1 and T2, produce a value V. Used to produce a value on cache miss.</param>
        public Cache(Func<T1, T2, V> createValue)
        {
            // Given a T2, produce a new nested cache that maps from T1 to Value
            Cache<T1, V> createNestedCache(T2 t2) => new(t1 => createValue(t1, t2));

            // This cache maps T2's to nested caches
            cache = new Cache<T2, Cache<T1, V>>(createNestedCache);
        }

        /// <summary>
        /// Return the value produced by createValue(param1, param2), either from cache or by calling the function.
        /// </summary>
        public V Get(T1 param1, T2 param2) => cache.Get(param2).Get(param1);

        /// <summary>
        /// Clear all entries in the cache
        /// </summary>
        public void Clear() => cache.Clear();
    }

    /// <summary>
    /// A cache of (T1, T2, T3) keys to V values.
    /// Keys can be
    /// 1. A primitive (int, string, etc)
    /// 2. An object that overrides GetHashCode etc properly
    /// 3. An IEnumerable of #1 or #2
    /// For best performance, T1 should have the largest spread, T3 the smallest. eg. T1 = 10^9 options, T2 = 10^6 options, T3 = 10^3 options.
    /// </summary>
    public class Cache<T1, T2, T3, V>
        where T1: notnull
        where T2: notnull
        where T3: notnull
    {
        private readonly Cache<T3, Cache<T1, T2, V>> cache;

        /// <summary>
        /// Initializes an empty cache with a function that produces values on cache miss.
        /// </summary>
        /// <param name="createValue">Given keys T1, T2, and T3, produce a value V. Used to produce a value on cache miss.</param>
        public Cache(Func<T1, T2, T3, V> createValue)
        {
            // Given a T3, produce a new nested cache that maps from (T1, T2) to Value
            Cache<T1, T2, V> createNestedCache(T3 t3) => new((t1, t2) => createValue(t1, t2, t3));

            // This cache maps T3's to nested caches
            cache = new Cache<T3, Cache<T1, T2, V>>(createNestedCache);
        }

        /// <summary>
        /// Return the value produced by createValue(param1, param2, param3), either from cache or by calling the function.
        /// </summary>
        public V Get(T1 param1, T2 param2, T3 param3) => cache.Get(param3).Get(param1, param2);
        
        /// <summary>
        /// Clear all entries in the cache
        /// </summary>
        public void Clear() => cache.Clear();
    }

    internal static class Cache
    {
        public static int HashList(IEnumerable list)
        {
            var hasher = new HashCode();
            foreach (var item in list)
            {
                hasher.Add(item);
            }
            return hasher.ToHashCode();
        }

        public static bool IsIEnumerable(Type type)
        {
            return type.IsGenericType &&
                type.GetInterface("IEnumerable") != null;
        }
    }
}