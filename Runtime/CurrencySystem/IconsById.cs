#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore
{
    public class IconsById : ValuesById<List<Sprite>>
    {
        [SerializeField] private CurrencyIdGroup allCurrenciesGroup;

        public static bool TryGetMainIcon(Id id, out Sprite sprite)
        {
            if (!Instance.ByKey.TryGetValue(id, out var sprites)
                || sprites.Count == 0
                || sprites[0] == null)
            {
                sprite = null;
                return false;
            }

            sprite = sprites[0];
            return true;
        }

        private static readonly SingleObject<IconsById> singleObject = new();
        public static IconsById Instance => singleObject.Get();
        protected override void SetupDataSelector(ValueDropdownList<Entry> list) => SetupByGroup(allCurrenciesGroup, list);

        protected override void OnValueProcessAttributes(List<Attribute> attributes)
        {
            attributes.Add(new PreviewFieldAttribute());
        }
    }
}
#endif