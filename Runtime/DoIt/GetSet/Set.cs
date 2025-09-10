using System;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class SetBuffer : DoIt
{
    [SerializeReference] public IGetRaw data;

    public override void Do()
    {
        var d = data.Data;
        DataBuffer.value = d;
    }
}

[Serializable]
public class SetKeyBuffer : DoIt
{
    public string key;
    [SerializeReference] public IGetRaw data;
    
    public override void Do()
    {
        var d = data.Data;
        DestroyEvent.AddOnDestroy(d, Remove);
        DataBuffer.Set(key, d);
    }
    
    private void Remove()
    {
        DataBuffer.Remove(key);
    }
}

[Serializable]
public class UnmanagedSetKeyBuffer : DoIt
{
    public string key;
    [SerializeReference] public IGetRaw data;

    public override void Do()
    {
        var d = data.Data;
        DataBuffer.Set(key, d);
    }
}

[Serializable]
public class RemoveKeyBuffer : DoIt
{
    public string key;

    public override void Do()
    {
        DataBuffer.Remove(key);
    }
}

[Serializable]
public abstract class Set<TValue> : DoIt
{
    public string propertyPath;
    [SerializeReference] public IGetRaw target;
    [SerializeReference] public Get<TValue> value;
}

[Serializable]
public class SetUnityObject : Set<Object>
{
    private ObjectPathAccessor accessor;

    public override void Do()
    {
        var t = target.Data;
        var v = value.Data;
        accessor ??= PathAccessor.GetRef(t, propertyPath);
        accessor.Set(t, v);
    }
}

[Serializable]
public abstract class Set : Set<object>
{
}

[Serializable]
public class SetClass : Set
{
    private ObjectPathAccessor accessor;

    public override void Do()
    {
        var t = target.Data;
        var v = value.Data;
        accessor ??= PathAccessor.GetRef(t, propertyPath);
        accessor.Set(t, v);
    }
}

[Serializable]
public class SetStruct<TValue> : DoIt where TValue : struct
{
    public string propertyPath;
    [SerializeReference] public IGetRaw target;
    [SerializeReference] public Get<TValue> value;
    private TypedPathAccessor<TValue> accessor;
    
    public override void Do()
    {
        var t = target.Data;
        var v = value.Data;
        accessor ??= PathAccessor.Get<TValue>(t, propertyPath);
        accessor.Set(t, v);
    }
}