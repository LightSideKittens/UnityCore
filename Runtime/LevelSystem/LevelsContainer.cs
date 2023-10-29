using System.Collections.Generic;
using LSCore.LevelSystem;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
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
        var level = CreateInstance<LevelConfig>();
        level.Container = this;
        level.name = $"{Id}_{AllLevels.Count + 1}";
        AssetDatabase.CreateAsset(level, $"{path}/{level.name}.asset");
        levels.Add(level);
    }
    
#endif
}