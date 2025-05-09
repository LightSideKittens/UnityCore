//-----------------------------------------------------------------------
// <copyright file="UnityObjectRootDrawer.cs" company="Sirenix ApS">
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

    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    [DrawerPriority(0, 100, 0)] // This should override most things, including the property context menu
    public sealed class UnityObjectRootDrawer<T> : OdinValueDrawer<T>
        where T : UnityEngine.Object
    {
        public static readonly bool IsGameObject = typeof(T) == typeof(GameObject);

        protected override bool CanDrawValueProperty(InspectorProperty property)
        {
            return property.IsTreeRoot;
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (IsGameObject)
            {
                SirenixEditorGUI.MessageBox("Odin does not currently have a full GameObject inspector window substitute implemented, so a GameObject cannot be directly inspected inline in the editor.");
                SirenixEditorFields.UnityObjectField(this.ValueEntry.SmartValue, typeof(GameObject), true);

                GUILayout.BeginHorizontal();
                {
                    GUIHelper.PushGUIEnabled(this.ValueEntry.SmartValue != null);

                    string text = this.ValueEntry.SmartValue != null ? ("Open Inspector window for " + this.ValueEntry.SmartValue.name) : "Open Inspector window (null)";

                    if (GUILayout.Button(GUIHelper.TempContent(text)))
                    {
                        GUIHelper.OpenInspectorWindow(this.ValueEntry.SmartValue);
                        GUIHelper.ExitGUI(true);
                    }

                    text = this.ValueEntry.SmartValue != null ? ("Select " + this.ValueEntry.SmartValue.name) : "Select GO (null)";

                    if (GUILayout.Button(GUIHelper.TempContent(text)))
                    {
                        Selection.activeObject = this.ValueEntry.SmartValue;
                        GUIHelper.ExitGUI(true);
                    }

                    GUIHelper.PopGUIEnabled();
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                var count = this.Property.Children.Count;

                for (int i = 0; i < count; i++)
                {
                    this.Property.Children[i].Draw();
                }
            }
        }
    }
}
#endif