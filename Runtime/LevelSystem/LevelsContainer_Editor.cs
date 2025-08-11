#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LSCore.Extensions;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public partial class LevelsContainer
{
    private readonly ValueDropdownList<string> selector = new();
    
    [ShowIf("isGameObject")]
    [ValueDropdown("selector")]
    [OnValueChanged("TargetTypeChanged")]
    [SerializeField] private string targetType;
    
    private string path;
    [UsedImplicitly] private bool isGameObject;

    private IEnumerable<Object> Levels
    {
        get
        {
            isGameObject = false;
            var allAssets = AssetDatabaseUtils.LoadAllAssets<Object>(paths: path);
            allAssets.Remove(this);
            if (allAssets.Count > 0)
            {
                if (allAssets[0] is GameObject obj)
                {
                    selector.Clear();
                    if (string.IsNullOrEmpty(targetType))
                    {
                        targetType = typeof(GameObject).AssemblyQualifiedName;
                    }
                    
                    isGameObject = true;
                    selector.Add(nameof(GameObject), typeof(GameObject).AssemblyQualifiedName);
                    var components = obj.GetComponents<Component>();
                    
                    for (int i = 0; i < components.Length; i++)
                    {
                        var type = components[i].GetType();
                        selector.Add(type.Name, type.AssemblyQualifiedName);
                    }
                }
            }
            
            OnTargetTypeChanged(allAssets);
            return allAssets;
        }
    }

    private void TargetTypeChanged()
    {
        OnTargetTypeChanged(levels);
    }

    private void OnTargetTypeChanged(List<Object> objects)
    {
        var type = Type.GetType(targetType);
        
        for (int i = 0; i < objects.Count; i++)
        {
            objects[i] = (Object)objects[i].Cast(type);
        }
    }

    [OnInspectorInit]
    private void Init()
    {
        path = this.GetFolderPath();

        for (int i = 0; i < levels.Count; i++)
        {
            var level = levels[i];
            if (level == null)
            {
                levels.RemoveAt(i);
                i--;
            }
        }
    }
    
    [Button]
    private void AddAll()
    {
        levels.Clear();
        foreach (var level in Levels)
        {
            levels.Add(level);
        }
    }

    [Button]
    private void UpdateName()
    {
        AssetDatabase.RenameAsset($"{path}/{name}.asset", $"{Id}_Container");
        foreach (var level in Levels)
        {
            var split = level.name.Split('_');
            AssetDatabase.RenameAsset($"{path}/{level.name}.asset", $"{Id}_{split[^1]}");
        }

        AssetDatabaseUtils.RenameFolder(path, Id);
    }
}
#endif