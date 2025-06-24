using System;
using System.Collections.Generic;

public class LSInvoker : DoIt
{
    public string key;
    
    public override void Do()
    {
        StringDict<Action>.Get(key)?.Invoke();
    }
}

public class LSSubscriber : DoIt
{
    public string key;
    public List<DoIt> actions;
    
    public override void Do()
    {
        var action = StringDict<Action>.Get(key);
        action += actions.Do;
        StringDict<Action>.Set(key, action);
    }
}