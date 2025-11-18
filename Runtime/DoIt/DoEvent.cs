using System;
using LSCore.Attributes;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class InvokeEvent : DoIt
{
    public string key;
    
    public override void Do()
    {
        DoEvent.Invoke(key);
    }
}

public static class DoEvent
{
    public static string GetKey(string key) => string.Concat("DoIt_Event", key);
    public static void Listen(string key, Action doIt)
    {
        var k = GetKey(key);
        StringDict<Action>.TryGet(k, out var action);
        action -= doIt;
        action += doIt;
        StringDict<Action>.Set(k, action);
    }
    
    public static void UnListen(string key, Action doIt)
    {
        var k = GetKey(key);
        StringDict<Action>.TryGet(k, out var action);
        action -= doIt;
        if (action == null)
        {
            StringDict<Action>.Remove(k);
        }
        else
        { 
            StringDict<Action>.Set(k, action);
        }
    }
    
    public static void Invoke(string key)
    {
        var k = DoEvent.GetKey(key);
        if (StringDict<Action>.TryGet(k, out var action))
        {
            if (action == null)
            {
                StringDict<Action>.Remove(k);
            }
            else
            { 
                action();
            }
        }
    }
}

[Serializable]
public class Listen : DoIt
{
    [GetContext] public Object owner;
    public string key;
    [SerializeReference] public DoIt doIt;
    private bool subscribed;
    
    public override void Do()
    {
        DoEvent.Listen(key, doIt.Do);
        if (owner != null && !subscribed)
        {
            subscribed = true;
            DestroyEvent.AddOnDestroy(owner, UnListen);
        }
    }

    public void UnListen()
    {
        DoEvent.UnListen(key, doIt.Do);
    }
}

[Serializable]
[Unwrap]
public class MultiListener : DoIt
{
    [Serializable]
    [Unwrap]
    public struct Data
    {
        public string key;
        [SerializeReference] public DoIt doIt;
    }
    
    [GetContext] public Object owner;
    [UnwrapTarget] public Data[] data;
    private bool subscribed;
    
    public override void Do()
    {
        for (int i = 0; i < data.Length; i++)
        {
            var d = data[i];
            DoEvent.Listen(d.key, d.doIt.Do);
        }

        if (owner != null && !subscribed)
        {
            subscribed = true;
            DestroyEvent.AddOnDestroy(owner, UnListen);
        }
    }

    public void UnListen()
    {
        for (int i = 0; i < data.Length; i++)
        {
            var d = data[i];
            DoEvent.UnListen(d.key, d.doIt.Do);
        }
    }
}