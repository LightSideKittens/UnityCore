using System;
using System.Collections.Generic;

public class LSInvoker : DoIt
{
    public string key;
    
    public override void Invoke()
    {
        StringDict<Action>.Get(key)?.Invoke();
    }
}

public class LSSubscriber : DoIt
{
    public string key;
    public List<DoIt> actions;
    
    public override void Invoke()
    {
        var action = StringDict<Action>.Get(key);
        action += actions.Invoke;
        StringDict<Action>.Set(key, action);
    }
}