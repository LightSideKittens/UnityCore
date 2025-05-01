//-----------------------------------------------------------------------
// <copyright file="OdinInspectorAboutWindow.cs" company="Sirenix ApS">
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

    using Sirenix.Utilities.Editor;
    using Sirenix.Utilities;
    using UnityEditor;
    using UnityEngine;
    using System;

    /// <summary>
    /// Adds menu items to the Unity Editor, draws the About window, and the preference window found under Edit > Preferences > Odin Inspector.
    /// </summary>
    public class OdinInspectorAboutWindow : EditorWindow
    {
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10f, 10f, this.position.width - 20f, this.position.height - 5f));

            string subtitle = OdinInspectorVersion.BuildName;

            SirenixEditorGUI.Title("Odin Inspector & Serializer", subtitle, TextAlignment.Left, true);

            if (OdinInspectorVersion.HasLicensee)
            {
                GUILayout.Label("Licensed to " + OdinInspectorVersion.Licensee, SirenixGUIStyles.LeftAlignedGreyMiniLabel);
            }

            DrawAboutGUI();
            GUILayout.EndArea();
            this.RepaintIfRequested();
        }

        // TODO: Make proper preferences GUI...

        //private static void OnPreferencesGUI()
        //{
        //    DrawAboutGUI();
        //    Rect rect = EditorGUILayout.GetControlRect();

        //    if (GUI.Button(new Rect(rect) { y = rect.y + 70f, height = 25f, }, "Get started using Odin"))
        //    {
        //        OdinGettingStartedWindow.ShowWindow();
        //    }

        //    if (GUI.Button(new Rect(rect) { y = rect.y + 70f + 30, height = 25f, }, "Show Odin Preferences"))
        //    {
        //        SirenixPreferencesWindow.OpenSirenixPreferences();
        //    }

        //    GUIHelper.RepaintIfRequested(GUIHelper.CurrentWindow);
        //}

        internal static void DrawAboutGUI()
        {
            Rect position = new Rect(EditorGUILayout.GetControlRect()) { height = 90f };

            // Logo
            GUI.DrawTexture(position.SetWidth(86).SetHeight(75).AddY(4).AddX(-5), EditorIcons.OdinInspectorLogo, ScaleMode.ScaleAndCrop);

            // About
            GUI.Label(new Rect(position) { x = position.x + 82f, y = position.y + 20f * 0f - 2f, height = 18f, }, OdinInspectorVersion.Version, SirenixGUIStyles.LeftAlignedGreyMiniLabel);
            GUI.Label(new Rect(position) { x = position.x + 82f, y = position.y + 20f * 1f - 2f, height = 18f, }, "Developed and published by Sirenix", SirenixGUIStyles.LeftAlignedGreyMiniLabel);
            GUI.Label(new Rect(position) { x = position.x + 82f, y = position.y + 20f * 2f - 2f, height = 18f, }, "All rights reserved", SirenixGUIStyles.LeftAlignedGreyMiniLabel);

            var linkStyle = EditorStyles.miniButton;
            float width = linkStyle.CalcSize(GUIHelper.TempContent("www.odininspector.com")).x;

            // Links
            DrawLink(new Rect(position) { x = position.xMax - width, y = position.y + 20f * 0f, width = width, height = 14f, }, "www.odininspector.com", "https://odininspector.com", linkStyle);
        }

        private static void DrawLink(Rect rect, string label, string link, GUIStyle style)
        {
            if (GUI.Button(rect, label, style))
            {
                Application.OpenURL(link);
            }
        }
    }
}
#endif