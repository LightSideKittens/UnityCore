//-----------------------------------------------------------------------
// <copyright file="ResultItemMetaDataDrawerPropertyResolver.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using System;
    using System.Collections.Generic;

    internal class ResultItemMetaDataDrawerPropertyResolver : BaseMemberPropertyResolver<ResultItemMetaDataDrawer>
    {
        protected override InspectorPropertyInfo[] GetPropertyInfos()
        {
            var value = this.ValueEntry.SmartValue;
            var array  = value.MetaData;
            var result = new List<InspectorPropertyInfo>(array.Length);

            bool hasExcludedFirstButton = false;

            for (int i = 0; i < array.Length; i++)
            {
                var item = array[i];
                if (item.Value is Delegate d)
                {
                    if (item.Value is Action && value.ExcludeFirstButton && !hasExcludedFirstButton)
                    {
                        hasExcludedFirstButton = true;
                        continue;
                    }

                    result.Add(InspectorPropertyInfo.CreateForDelegate("generated_" + item.Name + i, i, typeof(ResultItemMetaDataDrawer), d,
                        Combine(item.Attributes,
                            new ButtonAttribute(item.Name) { Expanded = true })));
                }
                else
                {
                    result.Add(InspectorPropertyInfo.CreateValue("generated_" + item.Name + i, i, SerializationBackend.None, MakeGetterSetter(i),
                        Combine(item.Attributes,
                            new ReadOnlyAttribute(),
                            (string.IsNullOrEmpty(item.Name) ? (Attribute)new HideLabelAttribute() : (Attribute)new LabelTextAttribute(item.Name)),
                            new EnableGUIAttribute(),
                            new HideReferenceObjectPickerAttribute())));
                }
            }

            return result.ToArray();
        }

        private IValueGetterSetter MakeGetterSetter(int i)
        {
            return new GetterSetter<ResultItemMetaDataDrawer, object>(
                (ref ResultItemMetaDataDrawer parent) => parent.MetaData[i].Value,
                null
            );
        }

        static Attribute[] Combine(Attribute[] a, params Attribute[] b)
        {
            if (a != null && a.Length > 0)
            {
                var combined = new Attribute[a.Length + b.Length];

                for (int i = 0; i < b.Length; i++)
                    combined[i] = b[i];
                
                for (int i = 0; i < a.Length; i++)
                    combined[i + b.Length] = a[i];


                return combined;
            }

            return b;
        }
    }
}
#endif