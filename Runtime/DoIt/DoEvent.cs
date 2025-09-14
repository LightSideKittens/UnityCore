using System;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class InvokeEvent : DoIt
{
    public string key;
    
    public override void Do()
    {
        var k = Listen.GetKey(key);
        if (StringDict<Action>.TryGet(k, out var action))
        {
            action();
        }
    }
}

[Serializable]
public class Listen : DoIt
{
    public static string GetKey(string key) => string.Concat("DoIt_Event", key);
    [GetContext] public Object owner;
    public string key;
    [SerializeReference] public DoIt doIt;
    private bool subscribed;
    
    public override void Do()
    {
        var k = GetKey(key);
        StringDict<Action>.TryGet(k, out var action);
        action += doIt.Do;
        StringDict<Action>.Set(k, action);
        if (owner != null && !subscribed)
        {
            subscribed = true;
            DestroyEvent.AddOnDestroy(owner, () =>
            {
                StringDict<Action>.Remove(k);
            });
        }
    }
}


[Serializable]
public class UnListen : DoIt
{
    public string key;
    
    public override void Do()
    {
        var k = Listen.GetKey(key);
        StringDict<Action>.Remove(k);
    }
}