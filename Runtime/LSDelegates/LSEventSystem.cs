using System;
using System.Collections.Generic;

public class LSInvoker : LSAction
{
    public string key;
    
    public override void Invoke()
    {
        StringDict<Action>.Get(key)?.Invoke();
    }
}

public class LSSubscriber : LSAction
{
    public string key;
    public List<LSAction> actions;
    
    public override void Invoke()
    {
        var action = StringDict<Action>.Get(key);
        action += actions.Invoke;
        StringDict<Action>.Set(key, action);
    }
}