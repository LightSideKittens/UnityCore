using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore
{
    public class Palette : ValueById<Color>
    {
        public static bool TryGet(Id id, out Color color)
        {
            var palette = SingleAsset<Palette>.Get("fr");
            return palette.ByKey.TryGetValue(id, out color);
        }
        
#if UNITY_EDITOR
        protected override void SetupDataSelector(ValueDropdownList<Entry> list)
        {
            SetupByIds(AllIdsByGroupType<PaletteIdGroup>(), list);
        }
#endif
    }
}