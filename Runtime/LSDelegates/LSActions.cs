using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;

[Serializable]
[TypeFilter("@DoItExtensions.Types")]
public abstract class DoIt
{
    public abstract void Invoke();
}

[Serializable]
public class DelegateDoIt : DoIt
{
    public Action action;

    public override void Invoke()
    {
        action();
    }
    
    public static explicit operator DelegateDoIt(Action action)
    {
        return new DelegateDoIt{action=action};
    }
    
    public static explicit operator Action(DelegateDoIt action)
    {
        return action.Invoke;
    }
}

[Serializable]
public class Log : DoIt
{
    public string message;
    
    public override void Invoke()
    {
        Burger.Log(message);
    }
}

public struct DataBuffer<T>
{
    public static T value;
}

public static class DoItExtensions
{
    public static void Invoke<T>(this DoIt action, T value)
    {
        DataBuffer<T>.value = value;
        action.Invoke();
    }
    
    public static void Invoke(this IEnumerable<DoIt> actions)
    {
        foreach (var action in actions)
        {
            action.Invoke();
        }
    }
    
    public static void Invoke<T>(this IEnumerable<DoIt> actions, T value)
    {
        foreach (var action in actions)
        {
            DataBuffer<T>.value = value;
            action.Invoke();
        }
    }
    
    public static void Invoke(this IList<DoIt> actions)
    {
        for (int i = 0; i < actions.Count; i++)
        {
            actions[i].Invoke();
        }
    }
    
    public static void Invoke<T>(this IList<DoIt> actions, T value)
    {
        for (int i = 0; i < actions.Count; i++)
        {
            DataBuffer<object>.value = value;
            DataBuffer<T>.value = value;
            actions[i].Invoke();
        }
    }
    
#if UNITY_EDITOR
    private static List<Type> types;
    public static List<Type> Types => types ??= GetAllDoItTypes();
    public static List<Type> GetAllDoItTypes()
    {
        var list = TypeCache.GetTypesDerivedFrom<DoIt>()
            .Where(x => !x.IsAbstract && !x.IsGenericTypeDefinition)
            .ToList();

        foreach (var genericArg in DataProviderUtility.GetAllProviderGenericArgs())
        { 
            list.Add(genericArg);
        }
        
        return list;
    }
#endif
}

