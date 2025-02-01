using System;
using System.Collections.Generic;

public class BiDictionary<TKey, TValue>
{
    private readonly Dictionary<TKey, TValue> valueByKey;
    private readonly Dictionary<TValue, TKey> keyByValue;

    public BiDictionary()
    {
        valueByKey = new Dictionary<TKey, TValue>();
        keyByValue = new Dictionary<TValue, TKey>();
    }
    
    public BiDictionary(IEqualityComparer<TKey> valueByKeyComparer, IEqualityComparer<TValue> keyByValueComparer)
    {
        valueByKey = new Dictionary<TKey, TValue>(valueByKeyComparer);
        keyByValue = new Dictionary<TValue, TKey>(keyByValueComparer);
    }
    
    public void Add(TKey key, TValue value)
    {
        if (valueByKey.ContainsKey(key))
        {
            throw new ArgumentException("Duplicate key");
        }

        if (keyByValue.ContainsKey(value))
        {
            throw new ArgumentException("Duplicate value");
        }
        
        valueByKey[key] = value;
        keyByValue[value] = key;
    }

    public bool RemoveFromKey(TKey key)
    {
        if (valueByKey.Remove(key, out TValue value))
        {
            keyByValue.Remove(value);
            return true;
        }
        return false;
    }

    public bool RemoveFromValue(TValue value)
    {
        if (keyByValue.Remove(value, out TKey key))
        {
            valueByKey.Remove(key);
            return true;
        }
        return false;
    }

    public bool TryGetValueFromKey(TKey key, out TValue value)
    {
        return valueByKey.TryGetValue(key, out value);
    }

    public bool TryGetKeyFromValue(TValue value, out TKey key)
    {
        return keyByValue.TryGetValue(value, out key);
    }

    public bool ContainsKey(TKey key)
    {
        return valueByKey.ContainsKey(key);
    }

    public bool ContainsValue(TValue value)
    {
        return keyByValue.ContainsKey(value);
    }

    public TValue this[TKey key]
    {
        get => valueByKey[key];
        set
        {
            valueByKey[key] = value;
            keyByValue[value] = key;
        }
    }

    public TKey this[TValue val]
    {
        get => keyByValue[val];
        set
        {
            keyByValue[val] = value;
            valueByKey[value] = val;
        }
    }

    public IEnumerable<TKey> Keys => valueByKey.Keys;
    public IEnumerable<TValue> Values => keyByValue.Keys;

    public void Clear()
    {
        keyByValue.Clear();
        valueByKey.Clear();
    }
}
