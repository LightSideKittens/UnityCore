using System.Collections.Generic;
using LSCore;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class SingleAsset<T> where T : Object
{
    private static readonly Dictionary<string, T> assetsById = new();
    
#if UNITY_EDITOR
    static SingleAsset()
    {
        World.Destroyed += assetsById.Clear;
    }
#endif
    
    public static T Get(string id)
    {
        if (assetsById.TryGetValue(id, out var asset)) return asset;
        asset = Addressables.LoadAssetAsync<T>(id).WaitForCompletion();
        assetsById.Add(id, asset);

        return asset;
    }
}
