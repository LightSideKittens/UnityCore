using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore
{
    public class IconsById : ValuesById<Sprite>
    {
        [SerializeField] private CurrencyIdGroup allCurrenciesGroup;

        protected override void SetupDataSelector(ValueDropdownList<Data> list) => SetupByGroup(allCurrenciesGroup, list);

        protected override void OnValueProcessAttributes(List<Attribute> attributes)
        {
            attributes.Add(new PreviewFieldAttribute());
        }

#if UNITY_EDITOR
        private static readonly SingleObject<IconsById> singleObject = new();
        public static IconsById Instance => singleObject.Get(x => x.Init());
#endif
    }
}