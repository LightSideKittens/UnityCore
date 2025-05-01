//-----------------------------------------------------------------------
// <copyright file="SirenixPreferencesWindow.cs" company="Sirenix ApS">
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

    using Serialization;
    using Sirenix.Utilities.Editor;
    using Sirenix.Utilities;
    using System;
    using UnityEngine;
    using Sirenix.OdinInspector.Editor.Modules;
    using System.Linq;
    using System.Reflection;
    using Sirenix.OdinInspector.Editor.Drawers;
    using Sirenix.Utilities.Editor.Expressions.Internal;
    using UnityEditor;

    /// <summary>
    /// Sirenix preferences window.
    /// </summary>
    public class SirenixPreferencesWindow : OdinMenuEditorWindow
    {
        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(true)
            {
                { "General",                    GeneralDrawerConfig.Instance},
                { "Editor Types",               InspectorConfig.Instance},
                { "Persistent Context Cache",   PersistentContextCache.Instance},
                { "Color Palettes",             ColorPaletteManager.Instance},
                { "Serialization",              GlobalSerializationConfig.Instance},
                { "Import Settings",            ImportSettingsConfig.Instance},
                { "AOT Generation",             AOTGenerationConfig.Instance},
                { "Editor Only Mode",           EditorOnlyModeConfig.Instance},
                { "Modules",                    OdinModuleConfig.Instance},
            };

            tree.Config.SelectMenuItemsOnMouseDown = true;

            return tree;
        }

        protected override void DrawMenu()
        {
            base.DrawMenu();
            var rect = GUIHelper.GetCurrentLayoutRect().Padding(4).AlignBottom(20);
            GUI.Label(rect, "Odin Inspector Version " + OdinInspectorVersion.Version, SirenixGUIStyles.CenteredGreyMiniLabel);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            this.DefaultLabelWidth = 278;
            this.ResizableMenuWidth = false;
        }

        /// <summary>
        /// Opens the Odin inspector preferences window.
        /// </summary>
        public static void OpenSirenixPreferences()
        {
            var window = GetWindow<SirenixPreferencesWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(900, 600);
            window.titleContent = new GUIContent("Preferences", EditorIcons.OdinInspectorLogo);
        }

        /// <summary>
        /// Opens the Odin inspector preferences window.
        /// </summary>
        public static void OpenWindow(object selectedItem)
        {
            var window = GetWindow<SirenixPreferencesWindow>();
            window.TrySelectMenuItemWithObject(selectedItem);
            window.titleContent = new GUIContent("Preferences", EditorIcons.OdinInspectorLogo);
        }
        
        internal void GotoPreferencesTab(string tabName)
        {
            UnityEditorEventUtility.DelayAction(() =>
            {
                OdinMenuItem generalTab = null;

                const string GENERAL_DRAWER_CONFIG_TAB_NAME = "General";

                for (var i = 0; i < this.MenuTree.MenuItems.Count; i++)
                {
                    OdinMenuItem item = this.MenuTree.MenuItems[i];

                    if (item.Name == GENERAL_DRAWER_CONFIG_TAB_NAME)
                    {
                        generalTab = item;
                        break;
                    }
                }

#if SIRENIX_INTERNAL
                if (generalTab == null)
                {
                    Debug.LogError($"Found no tab with the name: {GENERAL_DRAWER_CONFIG_TAB_NAME}.");
                }
#endif
                generalTab.Select();

                var generalConfig = generalTab.Value as GeneralDrawerConfig;

#if SIRENIX_INTERNAL
                if (!generalConfig)
                {
                    Debug.LogError($"Found the tab with the name {GENERAL_DRAWER_CONFIG_TAB_NAME} but the item.Value is not of the type {nameof(GeneralDrawerConfig)}, instead it is: {generalTab.Value.GetType()}.");
                }
#endif

                generalConfig.TargetTabName = tabName;
            });
        }
    }
}
#endif