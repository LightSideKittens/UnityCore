using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore
{
    public class Palette : ValueById<Color>
    {
        private static Palette instance;
        public void Init() => instance = this;

        public static bool TryGet(Id id, out Color color) => instance.ByKey.TryGetValue(id, out color);
        
#if UNITY_EDITOR
        protected override void SetupDataSelector(ValueDropdownList<Entry> list)
        {
            SetupByIds(AllIdsByGroupType<PaletteIdGroup>(), list);
        }
#endif
    }
}