using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore
{
    public class Palette : ValuesById<Color>
    {
        private static Palette instance;
        
        protected override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();
            instance = this;
        }

        public static bool TryGet(Id id, out Color color) => instance.ByKey.TryGetValue(id, out color);
        
#if UNITY_EDITOR
        protected override void SetupDataSelector(ValueDropdownList<Data> list)
        {
            SetupByIds(AllIdsByGroupType<PaletteIdGroup>(), list);
        }
#endif
    }
}