using LSCore;
using LSCore.LevelSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Battle.Data
{
    public class UnitsById : ValuesById<Unit>
    {
        [SerializeField] private LevelIdGroup group;

#if UNITY_EDITOR
        protected override void SetupDataSelector(ValueDropdownList<Entry> list) => SetupByGroup(group, list);
#endif
    }
}