//-----------------------------------------------------------------------
// <copyright file="SerializationInfoMenuItem.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    internal class SerializationInfoMenuItem : OdinMenuItem
    {
        private MemberSerializationInfo info;
        private string typeName;

        public const int IconSize = 20;
        public const int IconSpacing = 4;

        public SerializationInfoMenuItem(OdinMenuTree tree, string name, MemberSerializationInfo instance) : base(tree, name, instance)
        {
            this.info = instance;
            this.typeName = instance.MemberInfo.GetReturnType().GetNiceName();
        }

        protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
        {
            if (Event.current.type == EventType.Repaint)
            {
                {
                    labelRect.width -= 10;
                    float widthOfMemberName = SirenixGUIStyles.Label.CalcSize(GUIHelper.TempContent(this.Name)).x;
                    float widthOfTypeName = SirenixGUIStyles.RightAlignedGreyMiniLabel.CalcSize(GUIHelper.TempContent(this.typeName)).x;

                    GUI.Label(
                        labelRect
                            .SetX(Mathf.Max(labelRect.xMin + widthOfMemberName, labelRect.xMax - widthOfTypeName))
                            .SetXMax(labelRect.xMax), 
                        this.typeName, 
                        this.IsSelected ? SirenixGUIStyles.LeftAlignedWhiteMiniLabel : SirenixGUIStyles.LeftAlignedGreyMiniLabel);
                }

                rect.x += IconSpacing;
                rect.x += IconSpacing;
                rect = rect.AlignLeft(IconSize);
                rect = rect.AlignMiddle(IconSize);
                DrawTheIcon(rect, this.info.Info.HasAll(SerializationFlags.SerializedByOdin), this.info.OdinMessageType);
                rect.x += IconSpacing * 2 + IconSize;
                DrawTheIcon(rect, this.info.Info.HasAll(SerializationFlags.SerializedByUnity), this.info.UnityMessageType);
            }
        }

        private void DrawTheIcon(Rect rect, bool serialized, InfoMessageType messageType)
        {
            if (messageType == InfoMessageType.Error)
            {
                GUI.DrawTexture(rect.AlignCenterXY(22), EditorIcons.ConsoleErroricon, ScaleMode.ScaleToFit);
            }
            else if (messageType == InfoMessageType.Warning)
            {
                GUI.DrawTexture(rect.AlignCenterXY(20), EditorIcons.ConsoleWarnicon, ScaleMode.ScaleToFit);
            }
            else if (messageType == InfoMessageType.Info)
            {
                GUI.DrawTexture(rect.AlignCenterXY(20), EditorIcons.ConsoleInfoIcon, ScaleMode.ScaleToFit);
            }
            else if (serialized)
            {
                GUI.DrawTexture(rect.AlignCenterXY(EditorIcons.TestPassed.width), EditorIcons.TestPassed, ScaleMode.ScaleToFit);
            }
            else
            {
                GUI.color = EditorGUIUtility.isProSkin ? new Color(1, 1, 1, 0.2f) : new Color(0.15f, 0.15f, 0.15f, 0.2f);
                EditorIcons.X.Draw(rect.Padding(2));
                GUI.color = Color.white;
            }
        }
    }
}
#endif