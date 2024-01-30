using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore
{
    public class IconsById : ValueById<Sprite>
    {
        [SerializeField] private IdGroup allCurrenciesGroup;
        
        public bool TryGetIcon(Id id, out Sprite sprite) => ByKey.TryGetValue(id, out sprite);
        
#if UNITY_EDITOR
        protected override void SetupDataSelector(ValueDropdownList<Entry> list) => SetupByGroup(allCurrenciesGroup, list);
        protected override void OnValueProcessAttributes(List<Attribute> attributes)
        {
            attributes.Add(new PreviewFieldAttribute());
        }
#endif
    }
}