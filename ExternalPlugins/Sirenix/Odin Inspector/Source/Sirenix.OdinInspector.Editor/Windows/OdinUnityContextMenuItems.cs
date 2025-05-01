//-----------------------------------------------------------------------
// <copyright file="OdinUnityContextMenuItems.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using Sirenix.Utilities.Editor.Expressions;
using Sirenix.Utilities.Editor.Expressions.Internal;

namespace Sirenix.OdinInspector.Editor.Windows
{
#pragma warning disable

    using Sirenix.Utilities;
    using Sirenix.OdinValidator.Editor;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using Sirenix.Serialization;
    using Sirenix.OdinInspector.Editor.Internal;
    using System;
    using System.Linq;

    internal class OdinUnityContextMenuItems
    {
        const int Group0 = -1000;
        const int Group1 = 10000;
        const int Group2 = 100000;
        const int Group3 = 1000000;

        // ---------- GROUP 0 -------------
        [MenuItem("Tools/Odin/Getting Started", priority = Group0)]
        private static void OpenGettingStarted() => GettingStarted.GettingStartedWindow.ShowWindow();

        // ---------- GROUP 1 -------------
        [MenuItem("Tools/Odin/Inspector/Attribute Overview", priority = Group1)]
        public static void OpenAttributesOverview() => AttributesExampleWindow.OpenWindow(null);

        [MenuItem("Tools/Odin/Serializer/Serialization Debugger", priority = Group1)]
        public static void ShowSerializationDebugger() => SerializationDebuggerWindow.ShowWindow();

        [MenuItem("Tools/Odin/Serializer/Import Settings", priority = Group1)]
        public static void ShowSerializationImportSettings() => SirenixPreferencesWindow.OpenWindow(ImportSettingsConfig.Instance);

        [MenuItem("Tools/Odin/Serializer/AOT Generation", priority = Group1)]
        public static void ShowSerializationAOTGeneration() => SirenixPreferencesWindow.OpenWindow(AOTGenerationConfig.Instance);

        [MenuItem("Tools/Odin/Serializer/Preferences", priority = Group1)]
        public static void ShowSerializationPreferences() => SirenixPreferencesWindow.OpenWindow(GlobalSerializationConfig.Instance);

        //[MenuItem("Tools/Odin/Validator", true, priority = Group1)]
        //public static bool ShowValidatorValidate() => Sirenix.Serialization.TwoWaySerializationBinder.Default.BindToType("Sirenix.OdinValidator.Editor.OdinValidatorWindow") != null;

        [MenuItem("Tools/Odin/Validator", priority = Group1)]
        public static void ShowValidator()
        {
            var t = Sirenix.Serialization.TwoWaySerializationBinder.Default.BindToType("Sirenix.OdinValidator.Editor.OdinValidatorWindow");
            if (t != null)
            {
                var method = t.GetMethods(Flags.StaticPublic)
                    .FirstOrDefault(x => x.Name == "OpenWindow" && x.GetParameters().Length == 0);
                method.Invoke(null, null);
            }
        }

        [MenuItem("Tools/Odin/Validator", true, priority = Group1)]
        public static bool ShowValidatorValidate() => Sirenix.Serialization.TwoWaySerializationBinder.Default.BindToType("Sirenix.OdinValidator.Editor.OdinValidatorWindow") != null;

        [MenuItem("Tools/Odin/Inspector/Sdf Icon Overview", priority = Group1 + 2)]
        public static void OpenSdfIconOverview() => SdfIconOverviewWindow.ShowWindow();

        [MenuItem("Tools/Odin/Inspector/Unit Overview", priority = Group1 + 3)]
        public static UnitOverviewWindow OpenUnitOverview() => UnitOverviewWindow.ShowWindow();

        [MenuItem("Tools/Odin/Inspector/Static Inspector", priority = Group1 + 4)]
        private static void OpenStaticInspector() => StaticInspectorWindow.ShowWindow();

        [MenuItem("Tools/Odin/Inspector/Preferences", priority = Group1 + 5)]
        public static void OpenSirenixPreferences() => SirenixPreferencesWindow.OpenSirenixPreferences();


        // ---------- GROUP 2 -------------

        [MenuItem("Tools/Odin/Help/Discord", priority = Group3 + 1)]
        private static void Discord() => Application.OpenURL("https://discord.gg/WTYJEra");

        [MenuItem("Tools/Odin/Help/Report An Issue", priority = Group3 + 2)]
        private static void ReportAnIssue() => Application.OpenURL("https://bitbucket.org/sirenix/odin-inspector/issues");

        [MenuItem("Tools/Odin/Help/Contact", priority = Group3 + 3)]
        private static void Contact() => Application.OpenURL("https://odininspector.com/support");

        [MenuItem("Tools/Odin/Help/Release Notes", priority = Group3 + 4)]
        private static void OpenReleaseNotes() => Application.OpenURL("https://odininspector.com/patch-notes");

        [MenuItem("Tools/Odin/Help/Check for updates", priority = Group3 + 5)]
        private static void CheckForUpdates() => CheckForUpdatesWindow.OpenWindow();

        [MenuItem("Tools/Odin/Help/About", priority = Group3 + 6)]
        private static void ShowAboutOdinInspector()
        {
            var rect = GUIHelper.GetEditorWindowRect().AlignCenter(465f).AlignMiddle(OdinInspectorVersion.HasLicensee ? 150f : 135f);
            var w = OdinInspectorAboutWindow.GetWindowWithRect<OdinInspectorAboutWindow>(rect, true, "Odin Inspector & Serializer");
            w.ShowUtility();
        }


        // ---------- GROUP 3 -------------

#if ODIN_EVALUATION_VERSION
		[MenuItem("Tools/Odin/Get it here", priority = Group3)]
		private static void OpenStoreLink() => Application.OpenURL("https://odininspector.com/pricing");
#endif


        // ---------- CONTEXT -------------

        [MenuItem("CONTEXT/MonoBehaviour/Odin/Debug Serialization")]
        private static void ComponentContextMenuItem(MenuCommand menuCommand) => SerializationDebuggerWindow.ShowWindow(menuCommand.context.GetType());
    }
}
#endif