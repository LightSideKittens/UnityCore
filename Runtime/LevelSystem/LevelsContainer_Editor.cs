#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public partial class LevelsContainer
{
    public static readonly List<Type> availableTypes = new()
    {
        typeof(GameObject),
        typeof(ScriptableObject),
    };

    private readonly ValueDropdownList<int> selector = new();
    
    [ValueDropdown("selector")]
    [SerializeField] private int targetType;
    
    private string path;
    private IEnumerable<Object> Levels => AllLevels;
    private IEnumerable<Object> AllLevels
    {
        get
        {
            var type = availableTypes[targetType];
            
            if (typeof(Component).IsAssignableFrom(type))
            {
                return AssetDatabaseUtils.LoadAllGameObjects(type, paths: path);
            }
            
            return AssetDatabaseUtils.LoadAllAssets(availableTypes[targetType], paths: path);
        }
    }

    [OnInspectorInit]
    private void Init()
    {
        selector.Clear();
        for (int i = 0; i < availableTypes.Count; i++)
        {
            var type = availableTypes[i];
            selector.Add(type.Name, i);
        }
        
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