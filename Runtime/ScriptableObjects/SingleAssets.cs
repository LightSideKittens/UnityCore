using System.Collections.Generic;
using LSCore.DataStructs;
using Sirenix.OdinInspector;
using UnityEngine;

public class FromSingleAssetsAttribute : ValueDropdownAttribute
{
    public FromSingleAssetsAttribute() : base("@SingleAssets.Keys")
    {
        
    }
} 

public class SingleAssets : ScriptableObject
{
    public UniDict<string, Object> assets;
    private static SingleAssets instance;
    private static SingleAssets Instance => instance ?? Resources.Load<SingleAssets>("SingleAssets");
    
    public static IEnumerable<string> Keys => Instance.assets.Keys;
    public static T Get<T>(string key) where T : Object => (T)Instance.assets[key];
}
