using System;
using System.Collections.Generic;
using LSCore;
using LSCore.DataStructs;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

public class FromSingleAssetsAttribute : ValueDropdownAttribute
{
    public FromSingleAssetsAttribute() : base("@SingleAssets.Keys")
    {
        
    }
}

public class SingleAssets : ScriptableObject
{
    public UniDict<string, Object> assets;
    [OnValueChanged("OnTypeAssetsChanged")] public List<Object> typedAssets;
    private static SingleAssets instance;
    private Dictionary<Type, Object> typedAssetsDict = new();
    private static SingleAssets Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<SingleAssets>("SingleAssets");
                instance.Init();
            }

            return instance;
        }
    }

    public static IEnumerable<string> Keys => Instance.assets.Keys;
    public static T Get<T>(string key) where T : Object => (T)Instance.assets[key];
    public static T Get<T>() where T : Object => (T)Instance.typedAssetsDict[typeof(T)];

    private void Init()
    {
        typedAssetsDict.Clear();
        for (var i = 0; i < typedAssets.Count; i++)
        {
            typedAssetsDict.Add(typedAssets[i].GetType(), typedAssets[i]);
        }
        
        World.Destroyed += OnDestroy;

        void OnDestroy()
        {
            World.Destroyed -= OnDestroy;
            Resources.UnloadAsset(this);
            instance = null;
        }
    }
    
    private void OnTypeAssetsChanged()
    {
        typedAssetsDict.Clear();
        for (var i = 0; i < typedAssets.Count; i++)
        {
            if (typedAssetsDict.TryAdd(typedAssets[i].GetType(), typedAssets[i])) continue;
            typedAssets.RemoveAt(i--);
        }
    }
}
