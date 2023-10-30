using System.Collections.Generic;
using LSCore;
using LSCore.LevelSystem;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

public class LevelsContainer : SerializedScriptableObject
{
    [field: SerializeField, ReadOnly] public Id Id { get; set; } 
    
    [ValueDropdown("Levels", IsUniqueList = true)]
    [HideReferenceObjectPicker]
    public List<LevelConfig> levels = new();
    
    [field: HideInInspector, SerializeField] public LevelsManager Manager { get; set; }
    
#if UNITY_EDITOR
    private string path;
    private IEnumerable<LevelConfig> Levels => AllLevels;
    private List<LevelConfig> AllLevels => AssetDatabaseUtils.LoadAllAssets<LevelConfig>(paths: path);

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
    private void CreateLevel()
    {
        var number = 1;
        var targetName = $"{Id}_{number}";
        var paths = AssetDatabaseUtils.GetPaths<LevelConfig>(paths: path);
        for (int i = 0; i < paths.Length; i++)
        {
            var levelName = Path.GetFileNameWithoutExtension(paths[i]);
            
            if (levelName != targetName)
            {
                break;
            }
            
            targetName = $"{Id}_{++number}";
        }
        var level = CreateInstance<LevelConfig>();
        level.Container = this;
        level.name = targetName;
        AssetDatabase.CreateAsset(level, $"{path}/{targetName}.asset");
        levels.Insert(number - 1, level);
    }
    
#endif
}