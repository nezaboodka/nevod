//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Nezaboodka.Nevod
{
    public static class DictionaryExtension
    {
        public static void AddValueListItem<TKey, TValue>(this IDictionary<TKey, List<TValue>> dictionary,
            TKey key, TValue value)
        {
            if (dictionary.TryGetValue(key, out List<TValue> valueList))
                valueList.Add(value);
            else
                dictionary.Add(key, new List<TValue>() { value });
        }

        public static void AddValueListItem<TKey, TValue>(this IDictionary<TKey, ImmutableArray<TValue>> dictionary,
            TKey key, TValue value)
        {
            if (dictionary.TryGetValue(key, out ImmutableArray<TValue> valueList))
                dictionary[key] = new ImmutableArray<TValue>(valueList, value);
            else
                dictionary.Add(key, new ImmutableArray<TValue>(value));
        }

        public static Dictionary<TKey, List<TValue>> CloneWithValueListsNullable<TKey, TValue>(
            this Dictionary<TKey, List<TValue>> dictionary)
        {
            Dictionary<TKey, List<TValue>> result;
            if (dictionary != null)
                result = dictionary.ToDictionary(entry => entry.Key, entry => new List<TValue>(entry.Value), dictionary.Comparer);
            else
                result = null;
            return result;
        }

        public static bool TryGetValueNullable<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
            out TValue value)
        {
            bool result;
            if (dictionary != null)
                result = dictionary.TryGetValue(key, out value);
            else
            {
                result = false;
                value = default;
            }
            return result;
        }

        public static TValue GetValueOrDefaultNullable<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue result;
            dictionary.TryGetValueNullable(key, out result);
            return result;
        }

        public static TValue GetValueOrDefaultNullable<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
            TValue defaultValue)
        {
            TValue result;
            if (!dictionary.TryGetValueNullable(key, out result))
                result = defaultValue;
            return result;
        }

        public static Dictionary<TKey, List<TValue>> MergeFromNullable<TKey, TValue>(
            this Dictionary<TKey, List<TValue>> destination, Dictionary<TKey, List<TValue>> source,
            Func<Dictionary<TKey, List<TValue>>> dictionaryConstructor)
        {
            Dictionary<TKey, List<TValue>> result;
            if (source != null)
            {
                if (destination == null)
                    result = dictionaryConstructor();
                else
                    result = destination;
                foreach (KeyValuePair<TKey, List<TValue>> keyValuePair in source)
                {
                    if (result.TryGetValue(keyValuePair.Key, out List<TValue> expressionsList))
                        expressionsList.AddRange(keyValuePair.Value);
                    else
                        result[keyValuePair.Key] = new List<TValue>(keyValuePair.Value);
                }
            }
            else
                result = destination;
            return result;
        }

        public static Dictionary<TKey, ImmutableArray<TValue>> MergeFromNullable<TKey, TValue>(
            this Dictionary<TKey, ImmutableArray<TValue>> destination, Dictionary<TKey, ImmutableArray<TValue>> source,
            Func<Dictionary<TKey, ImmutableArray<TValue>>> dictionaryConstructor)
        {
            Dictionary<TKey, ImmutableArray<TValue>> result;
            if (source != null)
            {
                if (destination == null)
                    result = dictionaryConstructor();
                else
                    result = destination;
                foreach (KeyValuePair<TKey, ImmutableArray<TValue>> keyValuePair in source)
                {
                    if (result.TryGetValue(keyValuePair.Key, out ImmutableArray<TValue> expressionsList))
                        result[keyValuePair.Key] = new ImmutableArray<TValue>(expressionsList, keyValuePair.Value);
                    else
                        result[keyValuePair.Key] = new ImmutableArray<TValue>(keyValuePair.Value);
                }
            }
            else
                result = destination;
            return result;
        }

        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
            where TValue : new()
        {
            TValue result;
            if (!dictionary.TryGetValue(key, out result))
            {
                result = new TValue();
                dictionary[key] = result;
            }
            return result;
        }

#if !NETCOREAPP2_0
        public static bool Remove<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, out TValue removed)
        {
            bool result;
            if (dictionary.TryGetValue(key, out removed))
            {
                dictionary.Remove(key);
                result = true;
            }
            else
                result = false;
            return result;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.GetValueOrDefaultNullable(key);
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
           TValue defaultValue)
        {
            TValue result;
            if (!dictionary.TryGetValue(key, out result))
                result = defaultValue;
            return result;
        }

        public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            bool result;
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value);
                result = true;
            }
            else
                result = false;
            return result;
        }
#endif
    }
}
