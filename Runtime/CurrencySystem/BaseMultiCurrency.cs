using System;
using System.Collections.Generic;
using LSCore.ConfigModule;
using Newtonsoft.Json;

public abstract class BaseMultiCurrency<TKey, T> : BaseConfig<T> where T : BaseMultiCurrency<TKey, T>, new()
{
    protected override string FolderName => "Currencies";
    public static Dictionary<TKey, int> defaultValue = new Dictionary<TKey, int>();
    [JsonProperty] private Dictionary<TKey, int> value = new Dictionary<TKey, int>();
    public static event Action Changing;
    public static event Action Changed;

    public static int GetValue(TKey key)
    {
        return Config.value[key];
    }

    public static void SetValue(TKey key, int value)
    {
        if (Config.value[key] != value)
        {
            Changing?.Invoke();
            Config.value[key] = value;
            Changed?.Invoke();
        }
    }

    protected override void SetDefault()
    {
        base.SetDefault();
        value = new Dictionary<TKey, int>(defaultValue);
    }

    public static void Earn(TKey key, int value)
    {
        SetValue(key, GetValue(key) + value);
    }

    public static bool TrySpend(TKey key, int value)
    {
        var canSpend = value <= GetValue(key);

        if (canSpend)
        {
            SetValue(key, GetValue(key) - value);
        }

        return canSpend;
    }

    public static bool TryConvertTo<TKey1, T1>(TKey fromKey, int fromUnitCount, TKey1 toKey, int toUnitCount)
        where T1 : BaseMultiCurrency<TKey1, T1>, new()
    {
        var isSpent = TrySpend(fromKey, fromUnitCount);

        if (isSpent)
        {
            BaseMultiCurrency<TKey1, T1>.Earn(toKey, toUnitCount);
        }

        return isSpent;
    }
}