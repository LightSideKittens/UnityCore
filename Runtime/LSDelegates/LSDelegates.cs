using System;
using System.Collections.Generic;
using LSCore.Extensions.Unity;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
[HideReferenceObjectPicker]
public abstract class LSAction
{
    public abstract void Invoke();
}

[Serializable]
public class Log : LSAction
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

public static class LSActionExtensions
{
    public static void Invoke<T>(this LSAction action, T value)
    {
        DataBuffer<T>.value = value;
        action.Invoke();
    }
    
    public static void Invoke(this IEnumerable<LSAction> actions)
    {
        foreach (var action in actions)
        {
            action.Invoke();
        }
    }
    
    public static void Invoke<T>(this IEnumerable<LSAction> actions, T value)
    {
        foreach (var action in actions)
        {
            DataBuffer<T>.value = value;
            action.Invoke();
        }
    }
    
    public static void Invoke(this IList<LSAction> actions)
    {
        for (int i = 0; i < actions.Count; i++)
        {
            actions[i].Invoke();
        }
    }
    
    public static void Invoke<T>(this IList<LSAction> actions, T value)
    {
        for (int i = 0; i < actions.Count; i++)
        {
            DataBuffer<T>.value = value;
            actions[i].Invoke();
        }
    }
}