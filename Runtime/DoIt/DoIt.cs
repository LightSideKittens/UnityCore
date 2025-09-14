using System;
using System.Collections.Generic;
using LSCore.Attributes;
using LSCore.Extensions;
using UnityEngine;

[Serializable]
public abstract class DoIt
{
    public abstract void Do();
    public static implicit operator Action(DoIt doIt) => doIt != null ? doIt.Do : null;
}

[Serializable]
[Unwrap]
public class DoIts : DoIt
{
    [SerializeReference] public DoIt[] doIts;
    public override void Do() => doIts.Do();
}

[Serializable]
[Unwrap]
public class DoOnce : DoIt
{
    [SerializeReference] public DoIt doIt;
    private bool did;
    public override void Do()
    {
        if (did) return;
        doIt.Do();
        did = true;
    }
}


[Serializable]
public class DelegateDoIt : DoIt
{
    public Action action;

    public override void Do()
    {
        action();
    }
    
    public static explicit operator DelegateDoIt(Action action)
    {
        return new DelegateDoIt{action=action};
    }
    
    public static explicit operator Action(DelegateDoIt action)
    {
        return action.Do;
    }

    public override int GetHashCode() => action.GetHashCode();
    public override bool Equals(object obj) => action.Equals(obj);
    public override string ToString() => action.ToString();
    public static bool operator ==(DelegateDoIt a, DelegateDoIt b) => a.action == b.action;
    public static bool operator !=(DelegateDoIt a, DelegateDoIt b) => a.action != b.action;
    public static bool operator ==(DelegateDoIt a, Action b) => a.action == b;
    public static bool operator !=(DelegateDoIt a, Action b) => a.action != b;
    public static bool operator ==(Action a, DelegateDoIt b) => a == b.action;
    public static bool operator !=(Action a, DelegateDoIt b) => a != b.action;
}

[Serializable]
public class Log : DoIt
{
    public string message;
    
    public override void Do()
    {
        Burger.Log(message);
    }
}

public struct DataBuffer
{
    public static object value;
    public static Dictionary<string, object> map = new();
    public static T Get<T>() => value.Cast<T>();
    public static T Get<T>(string key) => map[key].Cast<T>();
    public static object GetRaw(string key) => map[key];
    public static void Set(string key, object val) => map[key] = val;
    public static bool Remove(string key) => map.Remove(key);
}

public static class DoItExtensions
{
    public static void Do<T>(this DoIt action, T value)
    {
        DataBuffer.value = value;
        action.Do();
    }
    
    public static void Do(this IEnumerable<DoIt> actions)
    {
        foreach (var action in actions)
        {
            action.Do();
        }
    }
    
    public static void Do<T>(this IEnumerable<DoIt> actions, T value)
    {
        foreach (var action in actions)
        {
            DataBuffer.value = value;
            action.Do();
        }
    }
    
    public static void Do(this IList<DoIt> actions)
    {
        for (int i = 0; i < actions.Count; i++)
        {
            actions[i].Do();
        }
    }
    
    public static void Do<T>(this IList<DoIt> actions, T value)
    {
        for (int i = 0; i < actions.Count; i++)
        {
            DataBuffer.value = value;
            actions[i].Do();
        }
    }
}

