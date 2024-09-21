using System.Collections;

namespace UnityBundleReader.Extensions;

public static class OrderedDictionaryExtensions
{
    public static Dictionary<object, object?> ToDictionary(this IDictionary dictionary) => ToDictionary<object, object?>(dictionary, k => k, v => v);

    public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IDictionary dictionary, Func<object, TKey> keySelector, Func<object?, TValue> valueSelector)
        where TKey: notnull
    {
        Dictionary<TKey, TValue> result = new();

        foreach (object? key in dictionary.Keys)
        {
            object? value = dictionary[key];

            TKey mappedKey = keySelector(key);
            TValue mappedValue = valueSelector(value);

            result[mappedKey] = mappedValue;
        }

        return result;
    }
}
