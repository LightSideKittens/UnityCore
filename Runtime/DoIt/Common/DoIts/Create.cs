using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class Create<T> : DoIt where T : Component
{
    public T prefab;
    [NonSerialized] public T obj;
        
    public override void Do()
    {
        obj = Object.Instantiate(prefab);
    }
}

[Serializable]
public class CreateSinglePrefab<T> : Create<T> where T : Component
{
    private static readonly Dictionary<T, T> objectByPrefab = new();
        
    public override void Do()
    {
        if (!objectByPrefab.TryGetValue(prefab, out obj))
        {
            base.Do();
            OnCreated();
            objectByPrefab.Add(prefab, obj);
        }
    }

    protected virtual void OnCreated()
    {
        var destroyEvent = obj.gameObject.AddComponent<DestroyEvent>();
        destroyEvent.Destroyed += () =>
        {
            objectByPrefab.Remove(prefab);
        };
    }
}