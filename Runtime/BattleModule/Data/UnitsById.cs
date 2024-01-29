using LSCore.LevelSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore.BattleModule
{
    public class UnitsById : ValuesById<Unit>
    {
        [SerializeField] private LevelIdGroup group;

#if UNITY_EDITOR
        protected override void SetupDataSelector(ValueDropdownList<Entry> list) => SetupByGroup(group, list);
#endif
    }
}