using System.Collections.Generic;

public static class DictionaryExtensions
{
    public static TValue As<TKey, TValue>(this Dictionary<TKey, TValue> token, TKey key) where TValue : new()
    {
        if (token.TryGetValue(key, out var val))
        {
            return val;
        }
        
        val = new TValue();
        token[key] = val;
        return val;
    }
}