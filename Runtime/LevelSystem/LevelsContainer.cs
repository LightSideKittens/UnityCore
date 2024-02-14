using System.Collections.Generic;
using LSCore;
using LSCore.LevelSystem;
using Sirenix.OdinInspector;
using UnityEngine;

public partial class LevelsContainer : ScriptableObject
{
    [field: SerializeField, ReadOnly] public Id Id { get; set; } 
    
    [ValueDropdown("Levels", IsUniqueList = true)]
    public List<Object> levels;
    
    [field: HideInInspector, SerializeField] public LevelsManager Manager { get; set; }
}