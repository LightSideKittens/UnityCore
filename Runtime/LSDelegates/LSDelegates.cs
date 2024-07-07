using System;
using System.Collections.Generic;
using LSCore.Attributes;
using Sirenix.OdinInspector;

[Serializable]
[HideReferenceObjectPicker]
[TypeFrom]
public abstract class LSAction
{
    private class Null : LSAction {public override void Invoke() { throw new NullReferenceException(); } }
    
    public abstract void Invoke();
}

[Serializable]
[HideReferenceObjectPicker]
[TypeFrom]
public abstract class LSAction<T>
{
    private class Null : LSAction<T> {public override void Invoke(T value) { throw new NullReferenceException(); } }
    public abstract void Invoke(T value);
}

[Serializable]
[HideReferenceObjectPicker]
[TypeFrom]
public abstract class LSFunc<TReturn>
{
    private class Null : LSFunc<TReturn> {public override TReturn Invoke() { throw new NullReferenceException(); } }
    public abstract TReturn Invoke();
}

[Serializable]
[HideReferenceObjectPicker]
[TypeFrom]
public abstract class LSFunc<TReturn, TArg>
{
    private class Null : LSFunc<TReturn, TArg> {public override TReturn Invoke(TArg arg) { throw new NullReferenceException(); } }
    public abstract TReturn Invoke(TArg arg);
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