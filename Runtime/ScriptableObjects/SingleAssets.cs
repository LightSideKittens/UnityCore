using System;
using System.Collections.Generic;
using LSCore;
using LSCore.DataStructs;
using LSCore.Extensions;
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
    [Serializable]
    public class TypedAssetData
    {
        [Serializable]
        public struct TypeStruct
        {
            [HideInInspector] public string type;
            [HideInInspector] public Object obj;

            public override string ToString()
            {
                return type;
            }
        }
        
        [InfoBox("This type already exists in Typed Assets", InfoMessageType.Error, "$isError")]
        [ValueDropdown("Types")] 
        [OnValueChanged("OnTypeChanged")]
        public TypeStruct type;
        
        [OnValueChanged("OnAssetChanged")] public Object asset;
        [NonSerialized] public bool isError;
        public Type Type => Type.GetType(type.type);

        private void OnAssetChanged()
        {
            if(asset == null) return;
            var type = asset.GetType();
            this.type.type = type.AssemblyQualifiedName;
            this.type.obj = asset;
        }

        private void OnTypeChanged()
        {
            asset = type.obj;
        }
        
        private IEnumerable<ValueDropdownItem<TypeStruct>> Types
        {
            get
            {
                if(asset == null) yield break;
                var type = asset.GetType();
                var objType = typeof(Object);
                if (type == typeof(GameObject))
                {
                    var str = type.AssemblyQualifiedName;
                    yield return new ValueDropdownItem<TypeStruct>(str, new TypeStruct() {type = str, obj = asset });
                    var comps = ((GameObject)asset).GetComponents<Component>();
                    for (int i = 0; i < comps.Length; i++)
                    {
                        type = comps[i].GetType();
                        while (objType.IsAssignableFrom(type))
                        {
                            str = type.AssemblyQualifiedName;
                            yield return new ValueDropdownItem<TypeStruct>(str, new TypeStruct() {type = str, obj = comps[i] });
                            type = type.BaseType;
                        }
                    }
                    
                    yield break;
                }
                
                while (objType.IsAssignableFrom(type))
                {
                    var str = type.AssemblyQualifiedName;
                    yield return new ValueDropdownItem<TypeStruct>(str, new TypeStruct() {type = str, obj = asset });
                    type = type.BaseType;
                }
            }
        }
    }
    
    public UniDict<string, Object> assets;
    [OnValueChanged("OnTypeAssetsChanged", true)] public List<TypedAssetData> typedAssets;
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
            typedAssetsDict.Add(typedAssets[i].Type, typedAssets[i].asset);
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
            typedAssets[i].isError = false;
            if(typedAssets[i].asset == null) continue;
            if (!typedAssetsDict.TryAdd(typedAssets[i].Type, typedAssets[i].asset))
            {
                typedAssets[i].isError = true;
            }
        }
    }
}
