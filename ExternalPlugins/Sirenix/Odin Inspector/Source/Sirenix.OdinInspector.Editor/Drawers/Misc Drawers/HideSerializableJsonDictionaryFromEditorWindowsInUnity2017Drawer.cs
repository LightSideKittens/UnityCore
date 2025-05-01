//-----------------------------------------------------------------------
// <copyright file="HideSerializableJsonDictionaryFromEditorWindowsInUnity2017Drawer.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using System;
    using UnityEngine;
    using Sirenix.OdinInspector.Editor;

    [DrawerPriority(9001, 0, 0)]
    internal class HideSerializableJsonDictionaryFromEditorWindowsInUnity2017Drawer<T> : OdinValueDrawer<T> where T : ScriptableObject
    {
        public override bool CanDrawTypeFilter(Type type)
        {
            return type.FullName == "UnityEditor.Experimental.UIElements.SerializableJsonDictionary";
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            var member = entry.Property.Info.GetMemberInfo();
            if (member.MemberType == System.Reflection.MemberTypes.Field && member.Name == "m_PersistentViewDataDictionary")
            {
                return;
            }

            this.CallNextDrawer(label);
        }
    }
}
#endif