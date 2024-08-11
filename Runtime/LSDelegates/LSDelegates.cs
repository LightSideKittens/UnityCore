using System;
using System.Collections.Generic;
using LSCore.Attributes;
using Sirenix.OdinInspector;

[Serializable]
[HideReferenceObjectPicker]
[TypeFrom]
public abstract class LSAction
{
    public abstract void Invoke();
}

[Serializable]
[HideReferenceObjectPicker]
[TypeFrom]
public abstract class LSAction<T>
{
    public abstract void Invoke(T value);
}

public static class LSDelegatesExtensions
{
    public static void Invoke(this IEnumerable<LSAction> actions)
    {
        foreach (var action in actions)
        {
            action.Invoke();
        }
    }
    
    public static void Invoke<T>(this IEnumerable<LSAction<T>> actions, T value)
    {
        foreach (var action in actions)
        {
            action.Invoke(value);
        }
    }
}