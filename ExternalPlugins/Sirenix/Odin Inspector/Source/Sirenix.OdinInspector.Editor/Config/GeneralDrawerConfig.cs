//-----------------------------------------------------------------------
// <copyright file="GeneralDrawerConfig.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
#pragma warning disable 0414

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Serialization;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using Sirenix.OdinInspector.Editor.Drawers;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// <para>Contains general configuration for all Odin drawers.</para>
    /// <para>
    /// You can modify the configuration in the Odin Preferences window found in 'Tools -> Odin Inspector -> Preferences -> Drawers -> General',
    /// or by locating the configuration file stored as a serialized object in the Sirenix folder under 'Odin Inspector/Config/Editor/GeneralDrawerConfig'.
    /// </para>
    /// </summary>
    [InitializeOnLoad]
    [SirenixEditorConfig]
    [Searchable]
    [OnStateUpdate("@this.GotoTargetTabName(#(#_DefaultTabGroup))")]
    public class GeneralDrawerConfig : GlobalConfig<GeneralDrawerConfig>
    {
        internal string TargetTabName = null;
        
        [SerializeField, HideInInspector]
        private bool enableUIToolkitSupport = true;

        private ObjectFieldAlignment? squareUnityObjectAlignment;
        private UnityObjectType? squareUnityObjectEnableFor;
        private QuaternionDrawMode? quaternionDrawMode;
        private float? squareUnityObjectFieldHeight;
        private bool? showPrefabModificationsDisabledMessage;
        private bool? hidePagingWhileOnlyOnePage;
        private bool? useNewImprovedEnumDropdown;
        private bool? hidePagingWhileCollapsed;
        private bool? showMonoScriptInEditor;
        private bool? hideFoldoutWhileEmpty;
        private bool? showPagingInTables;
        private bool? openListsByDefault;
        private bool? drawEnumTypeTitle;
        private bool? showExpandButton;
        private bool? showIndexLabels;
        private bool? showItemCount;
        private int? maxRecursiveDrawDepth;
        private int? numberOfItemsPerPage;
        private bool? precomputeTypeMatching;
        private bool? showPrefabModifiedValueBar;
        private bool? enableSmartNumberFields;

        private int? buttonHeight;
        private IconAlignment? buttonIconAlignment;
        private bool? stretchButtons;
        private float? buttonAlignment;

        private static readonly ColorPref listItemColorEvenDarkSkinPref = new ColorPref("GeneralDrawerConfig.ListItemColorEvenDarkSkin", new Color(0.235f, 0.235f, 0.235f, 1f));
        private static readonly ColorPref listItemColorEvenLightSkinPref = new ColorPref("GeneralDrawerConfig.ListItemColorEvenLightSkin", new Color(0.838f, 0.838f, 0.838f, 1.000f));
        private static readonly ColorPref listItemColorOddDarkSkinPref = new ColorPref("GeneralDrawerConfig.ListItemColorOddDarkSkin", new Color(0.2f, 0.2f, 0.2f, 1f));
        private static readonly ColorPref listItemColorOddLightSkinPref = new ColorPref("GeneralDrawerConfig.ListItemColorOddLightSkin", new Color(0.788f, 0.788f, 0.788f, 1.000f));

		#region General

		private static string AllowUIToolkitSupportSuffix { get { return AllowUIToolkitSupport ? "" : "The UIToolkit support feature is only available in Unity 2020.2 and up"; } }
		private static bool AllowUIToolkitSupport { get { return Internal.UIToolkitIntegration.ImguiElementUtils.IsSupported; } }

		/// <summary>
		/// Specify whether or not the script selector above components should be drawn.
		/// </summary>
		[TabGroup("General"), ShowInInspector]
		[PropertyTooltip("Specify whether or not the UI toolkit support feature should be enabled or not. Note that the DrawWithVisualElements attribute will still work (in Unity 2020.2+) if this is disabled.")]
        [EnableIf(nameof(AllowUIToolkitSupport))]
        [SuffixLabel("$" + nameof(AllowUIToolkitSupportSuffix), false)]
		public bool EnableUIToolkitSupport
		{
			get
			{
                if (!Internal.UIToolkitIntegration.ImguiElementUtils.IsSupported)
                {
                    return false;
                }

                return this.enableUIToolkitSupport;
			}
			set
			{
				if (Internal.UIToolkitIntegration.ImguiElementUtils.IsSupported)
				{
				    this.enableUIToolkitSupport = value;
				}
			}
		}

		/// <summary>
		/// Specify whether or not the script selector above components should be drawn.
		/// </summary>
		[TabGroup("General"), ShowInInspector]
        [PropertyTooltip("Specify whether or not the script selector above components should be drawn")]
        public bool ShowMonoScriptInEditor
        {
            get
            {
                if (this.showMonoScriptInEditor == null)
                {
                    this.showMonoScriptInEditor = EditorPrefs.GetBool("GeneralDrawerConfig.ShowMonoScriptInEditor", true);
                }

                return this.showMonoScriptInEditor.Value;
            }
            set
            {
                this.showMonoScriptInEditor = value;
                EditorPrefs.SetBool("GeneralDrawerConfig.ShowMonoScriptInEditor", value);
            }
        }

        /// <summary>
        /// Specify whether or not the warning for properties that do not support prefab modifications should be shown in the inspector.
        /// </summary>
        [TabGroup("General"), ShowInInspector]
        [PropertyTooltip("Specify whether or not the warning for properties that do not support prefab modifications should be shown in the inspector")]
        public bool ShowPrefabModificationsDisabledMessage
        {
            get
            {
                if (this.showPrefabModificationsDisabledMessage == null)
                {
                    this.showPrefabModificationsDisabledMessage = EditorPrefs.GetBool("GeneralDrawerConfig.ShowPrefabModificationsDisabledMessage", true);
                }

                return this.showPrefabModificationsDisabledMessage.Value;
            }
            set
            {
                this.showPrefabModificationsDisabledMessage = value;
                EditorPrefs.SetBool("GeneralDrawerConfig.ShowPrefabModificationsDisabledMessage", value);
            }
        }

        /// <summary>
        /// Specify whether or not the warning for properties that do not support prefab modifications should be shown in the inspector.
        /// </summary>
        [TabGroup("General"), ShowInInspector]
        [PropertyTooltip("Specify whether or not a blue bar should be drawn next to modified prefab values")]
        [LabelText("Show Blue Prefab Value Modified Bar")]
        public bool ShowPrefabModifiedValueBar
        {
            get
            {
                if (this.showPrefabModifiedValueBar == null)
                {
                    this.showPrefabModifiedValueBar = EditorPrefs.GetBool("GeneralDrawerConfig.ShowPrefabModifiedValueBar", true);
                }

                return this.showPrefabModifiedValueBar.Value;
            }
            set
            {
                this.showPrefabModifiedValueBar = value;
                EditorPrefs.SetBool("GeneralDrawerConfig.ShowPrefabModifiedValueBar", value);
            }
        }

        /// <summary>
        /// Specifies the maximum depth to which a property can draw itself recursively before the system refuses to draw it any deeper.
        /// </summary>
        [TabGroup("General"), ShowInInspector]
        [PropertyTooltip("Specifies the maximum depth to which a property can draw itself recursively before the system refuses to draw it any deeper.")]
        [MinValue(1)]
        [MaxValue(100)]
        public int MaxRecursiveDrawDepth
        {
            get
            {
                if (this.maxRecursiveDrawDepth == null)
                {
                    this.maxRecursiveDrawDepth = EditorPrefs.GetInt("GeneralDrawerConfig.MaxRecursiveDrawDepth", 10);
                }

                return Mathf.Clamp(this.maxRecursiveDrawDepth.Value, 1, 100);
            }
            set
            {
                value = Mathf.Clamp(value, 1, 100);
                this.maxRecursiveDrawDepth = value;
                EditorPrefs.SetInt("GeneralDrawerConfig.MaxRecursiveDrawDepth", value);
            }
        }

        /// <summary>
        /// If set to true, most foldouts throughout the inspector will be expanded by default.
        /// </summary>
        [TabGroup("General"), ShowInInspector]
        [PropertyTooltip("If set to true, most foldouts throughout the inspector will be expanded by default.")]
        public bool ExpandFoldoutByDefault
        {
            get { return SirenixEditorGUI.ExpandFoldoutByDefault; }
            set { SirenixEditorGUI.ExpandFoldoutByDefault = value; }
        }

        /// <summary>
        /// If set to true, buttons will show the result values from invoking them in the inspector by default.
        /// </summary>
        [TabGroup("General"), ShowInInspector]
        [PropertyTooltip("If set to true, buttons will show the result values from invoking them in the inspector by default.")]
        public bool ShowButtonResultsByDefault
        {
            get { return SirenixEditorGUI.ShowButtonResultsByDefault; }
            set { SirenixEditorGUI.ShowButtonResultsByDefault = value; }
        }


        [TabGroup("General"), ShowInInspector, DelayedProperty]
        [PropertyTooltip("Enables Odin smart fields in the inspector.")]
        [MinValue(1)]
        [MaxValue(100)]
        public bool EnableSmartNumberFields
        {
            get
            {
                if (this.enableSmartNumberFields == null)
                {
                    this.enableSmartNumberFields = EditorPrefs.GetBool("GeneralDrawerConfig.EnableSmartNumberFields", true);
                }

                return this.enableSmartNumberFields.Value;
            }
            set
            {
                this.enableSmartNumberFields = value;
                EditorPrefs.SetBool("GeneralDrawerConfig.EnableSmartNumberFields", value);
            }
        }

        [TabGroup("General"), ShowInInspector, DelayedProperty]
        [PropertyTooltip("Specifies the number of expressions to keep for smart field history.")]
        [MinValue(1)]
        [MaxValue(100)]
        public int SmartFieldHistoryLength
        {
            get { return SirenixEditorFields.expressionHistory.MaxLength; }
            set { SirenixEditorFields.expressionHistory.SetMaxLength(value); }
        }

        ///// <summary>
        ///// If set to true, type matching for things such as drawers will be precomputed in a separate thread based on a cache from the previously loaded AppDomain that is stored on disk in the Project's Temp folder, resulting in some overall speedups in some cases when doing first-time type matching. Requires a recompile/project reload before it takes any effect.
        ///// </summary>
        //[TabGroup("General"), ShowInInspector]
        //[PropertyTooltip("If set to true, type matching for things such as drawers will be precomputed in a separate thread based on a cache from the previously loaded AppDomain that is stored on disk in the Project's Temp folder, resulting in some overall speedups in some cases when doing first-time type matching. Requires a recompile/project reload before it takes any effect."), SuffixLabel("EXPERIMENTAL")]
        //public bool PrecomputeTypeMatching
        //{
        //    get
        //    {
        //        if (this.precomputeTypeMatching == null)
        //        {
        //            this.precomputeTypeMatching = EditorPrefs.GetBool("GeneralDrawerConfig.PrecomputeTypeMatching", false);
        //        }

        //        return precomputeTypeMatching.Value;
        //    }
        //    set
        //    {
        //        this.precomputeTypeMatching = value;
        //        EditorPrefs.SetBool("GeneralDrawerConfig.PrecomputeTypeMatching", value);
        //    }
        //}


#endregion

#region Animations

        /// <summary>
        /// Specify the animation speed for most foldouts throughout the inspector.
        /// </summary>
        [TabGroup("Animations"), ShowInInspector]
        [PropertyRange(0.001f, 4f)]
        [PropertyTooltip("Specify the animation speed for most foldouts throughout the inspector.")]
        public float GUIFoldoutAnimationDuration
        {
            get { return SirenixEditorGUI.DefaultFadeGroupDuration; }
            set { SirenixEditorGUI.DefaultFadeGroupDuration = value; }
        }

        /// <summary>
        /// Specify the shaking duration for most shaking animations throughout the inspector.
        /// </summary>
        [TabGroup("Animations"), ShowInInspector]
        [PropertyTooltip("Specify the shaking duration for most shaking animations throughout the inspector.")]
        [PropertyRange(0f, 4f)]
        public float ShakingAnimationDuration
        {
            get { return SirenixEditorGUI.ShakingAnimationDuration; }
            set { SirenixEditorGUI.ShakingAnimationDuration = value; }
        }

        /// <summary>
        /// Specify the animation speed for <see cref="Sirenix.OdinInspector.TabGroupAttribute"/>
        /// </summary>
        [TabGroup("Animations"), ShowInInspector]
        [PropertyRange(0.001f, 4f)]
        public float TabPageSlideAnimationDuration
        {
            get { return SirenixEditorGUI.TabPageSlideAnimationDuration; }
            set { SirenixEditorGUI.TabPageSlideAnimationDuration = value; }
        }

#endregion

#region Structs

        /// <summary>
        /// When <c>true</c> the component labels, for vector fields, will be hidden when the field is too narrow.
        /// </summary>
        [TabGroup("Structs"), ShowInInspector]
        [PropertyTooltip("When on the component labels, for vector fields, will be hidden when the field is too narrow.\nThis allows more space for the actual component fields themselves.")]
        public bool ResponsiveVectorComponentFields
        {
            get { return SirenixEditorFields.ResponsiveVectorComponentFields; }
            set { SirenixEditorFields.ResponsiveVectorComponentFields = value; }
        }

        /// <summary>
        /// Specify how the Quaternion struct should be shown in the inspector.
        /// </summary>
        [TabGroup("Structs"), ShowInInspector]
        [EnumToggleButtons]
        [PropertyTooltip("Current mode for how quaternions are edited in the inspector.\n\nEuler: Rotations as yaw, pitch and roll.\n\nAngle axis: Rotations as a axis of rotation, and an angle of rotation around that axis.\n\nRaw: Directly edit the x, y, z and w components of a quaternion.")]
        public QuaternionDrawMode QuaternionDrawMode
        {
            get
            {
                if (this.quaternionDrawMode == null)
                {
                    this.quaternionDrawMode = (QuaternionDrawMode)EditorPrefs.GetInt("GeneralDrawerConfig.QuaternionDrawMode", (int)QuaternionDrawMode.Eulers);
                }

                return this.quaternionDrawMode.Value;
            }
            set
            {
                this.quaternionDrawMode = value;
                EditorPrefs.SetInt("GeneralDrawerConfig.QuaternionDrawMode", (int)value);
            }
        }

        [TabGroup("Structs"), ShowInInspector]
        private Quaternion ExampleQuaternion { get; set; }

        [TabGroup("Structs"), ShowInInspector]
        private Vector3 ExampleVector { get; set; }

#endregion

#region Enums

        /// <summary>
        /// Gets or sets a value indicating whether [use improved enum drop down].
        /// </summary>
        [TabGroup("Enums"), ShowInInspector]
        public bool UseImprovedEnumDropDown
        {
            get
            {
                if (this.useNewImprovedEnumDropdown == null)
                {
                    this.useNewImprovedEnumDropdown = EditorPrefs.GetBool("GeneralDrawerConfig.UseImprovedEnumDropDown", true);
                }

                return this.useNewImprovedEnumDropdown.Value;
            }
            set
            {
                this.useNewImprovedEnumDropdown = value;
                EditorPrefs.SetBool("GeneralDrawerConfig.UseImprovedEnumDropDown", value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [use improved enum drop down].
        /// </summary>
        [TabGroup("Enums"), ShowInInspector]
        [EnableIf("UseImprovedEnumDropDown")]
        public bool DrawEnumTypeTitle
        {
            get
            {
                if (this.drawEnumTypeTitle == null)
                {
                    this.drawEnumTypeTitle = EditorPrefs.GetBool("GeneralDrawerConfig.DrawEnumTypeTitle", false);
                }

                return this.drawEnumTypeTitle.Value;
            }
            set
            {
                this.drawEnumTypeTitle = value;
                EditorPrefs.SetBool("GeneralDrawerConfig.DrawEnumTypeTitle", value);
            }
        }

        [TabGroup("Enums"), ShowInInspector]
        private KeyCode ExampleEnum { get; set; }

        [TabGroup("Enums"), ShowInInspector]
        private AssemblyCategory ExampleFlagEnum { get; set; }
#endregion

#region Collections

        /// <summary>
        /// Specify whether or not a list should hide the foldout triangle when the list is empty.
        /// </summary>
        [InfoBox("All collection settings - and more - can be overridden for individual collections using the ListDrawerSettings attribute.")]
        [TabGroup("Collections"), ShowInInspector]
        [PropertyTooltip("Specifies whether all tables should include paging, or if the entirety of the table should be drawn as a list.")]
        public bool ShowPagingInTables
        {
            get
            {
                if (this.showPagingInTables == null)
                {
                    this.showPagingInTables = EditorPrefs.GetBool("GeneralDrawerConfig.ShowPagingInTables", false);
                }

                return this.showPagingInTables.Value;
            }
            set
            {
                this.showPagingInTables = value;
                EditorPrefs.SetBool("GeneralDrawerConfig.ShowPagingInTables", value);
            }
        }

        /// <summary>
        /// Specifies whether a list should hide the foldout triangle when the list is empty.
        /// </summary>
        //[PropertyTooltip("Specifies whether a list should hide the foldout triangle when the list is empty.")]
        [Obsolete("This setting no longer has any effect.", Consts.IsSirenixInternal)]
        public bool HideFoldoutWhileEmpty
        {
            get
            {
                if (this.hideFoldoutWhileEmpty == null)
                {
                    this.hideFoldoutWhileEmpty = EditorPrefs.GetBool("GeneralDrawerConfig.HideFoldoutWhileEmpty", true);
                }

                return this.hideFoldoutWhileEmpty.Value;
            }
            set
            {
                this.hideFoldoutWhileEmpty = value;
                EditorPrefs.SetBool("GeneralDrawerConfig.HideFoldoutWhileEmpty", value);
            }
        }

        /// <summary>
        /// Specify whether or not lists should hide the paging buttons when the list is collapsed.
        /// </summary>
        [TabGroup("Collections"), ShowInInspector]
        [PropertyTooltip("Specify whether or not lists should hide the paging buttons when the list is collapsed.")]
        public bool HidePagingWhileCollapsed
        {
            get
            {
                if (this.hidePagingWhileCollapsed == null)
                {
                    this.hidePagingWhileCollapsed = EditorPrefs.GetBool("GeneralDrawerConfig.HidePagingWhileCollapsed", true);
                }

                return this.hidePagingWhileCollapsed.Value;
            }
            set
            {
                this.hidePagingWhileCollapsed = value;
                EditorPrefs.SetBool("GeneralDrawerConfig.HidePagingWhileCollapsed", value);
            }
        }

        /// <summary>
        /// Specify whether or not lists should hide the paging buttons when there is only one page.
        /// </summary>
        [TabGroup("Collections"), ShowInInspector]
        public bool HidePagingWhileOnlyOnePage
        {
            get
            {
                if (this.hidePagingWhileOnlyOnePage == null)
                {
                    this.hidePagingWhileOnlyOnePage = EditorPrefs.GetBool("GeneralDrawerConfig.HidePagingWhileOnlyOnePage", true);
                }

                return this.hidePagingWhileOnlyOnePage.Value;
            }
            set
            {
                this.hidePagingWhileOnlyOnePage = value;
                EditorPrefs.SetBool("GeneralDrawerConfig.HidePagingWhileOnlyOnePage", value);
            }
        }

        /// <summary>
        /// Specify the number of elements drawn per page.
        /// </summary>
        [TabGroup("Collections"), ShowInInspector]
        [OnValueChanged("ResizeExampleList"), MaxValue(500), MinValue(2)]
        [PropertyTooltip("Specify the number of elements drawn per page.")]
        [LabelText("Number Of Items Per Page")]
        public int NumberOfItemsPrPage
        {
            get
            {
                if (this.numberOfItemsPerPage == null)
                {
                    this.numberOfItemsPerPage = EditorPrefs.GetInt("GeneralDrawerConfig.NumberOfItemsPrPage", 15);
                }

                return this.numberOfItemsPerPage.Value;
            }
            set
            {
                this.numberOfItemsPerPage = value;
                EditorPrefs.SetInt("GeneralDrawerConfig.NumberOfItemsPrPage", value);
            }
        }

        /// <summary>
        /// Specify whether or not lists should be expanded or collapsed by default.
        /// </summary>
        [TabGroup("Collections"), ShowInInspector]
        [PropertyTooltip("Specify whether or not lists should be expanded or collapsed by default.")]
        public bool OpenListsByDefault
        {
            get
            {
                if (this.openListsByDefault == null)
                {
                    this.openListsByDefault = EditorPrefs.GetBool("GeneralDrawerConfig.OpenListsByDefault", false);
                }

                return this.openListsByDefault.Value;
            }
            set
            {
                this.openListsByDefault = value;
                EditorPrefs.SetBool("GeneralDrawerConfig.OpenListsByDefault", value);
            }
        }

        /// <summary>
        /// Specify whether or not to include a button which expands the list, showing all pages at once.
        /// </summary>
        [TabGroup("Collections"), ShowInInspector]
        [PropertyTooltip("Specify whether or not to include a button which expands the list, showing all pages at once")]
        public bool ShowExpandButton
        {
            get
            {
                if (this.showExpandButton == null)
                {
                    this.showExpandButton = EditorPrefs.GetBool("GeneralDrawerConfig.ShowExpandButton", true);
                }

                return this.showExpandButton.Value;
            }
            set
            {
                this.showExpandButton = value;
                EditorPrefs.SetBool("GeneralDrawerConfig.ShowExpandButton", value);
            }
        }

        /// <summary>
        /// Specify whether or not lists should show item count.
        /// </summary>
        [TabGroup("Collections"), ShowInInspector]
        [PropertyTooltip("Specify whether or not lists should show item count.")]
        public bool ShowItemCount
        {
            get
            {
                if (this.showItemCount == null)
                {
                    this.showItemCount = EditorPrefs.GetBool("GeneralDrawerConfig.ShowItemCount", true);
                }

                return this.showItemCount.Value;
            }
            set
            {
                this.showItemCount = value;
                EditorPrefs.SetBool("GeneralDrawerConfig.ShowItemCount", value);
            }
        }

        /// <summary>
        /// Specify whether or not lists should show item count.
        /// </summary>
        [TabGroup("Collections"), ShowInInspector]
        [PropertyTooltip("Specify whether or not lists should show item count.")]
        public bool ShowIndexLabels
        {
            get
            {
                if (this.showIndexLabels == null)
                {
                    this.showIndexLabels = EditorPrefs.GetBool("GeneralDrawerConfig.ShowIndexLabels", false);
                }

                return this.showIndexLabels.Value;
            }
            set
            {
                this.showIndexLabels = value;
                EditorPrefs.SetBool("GeneralDrawerConfig.ShowIndexLabels", value);
            }
        }

        /// <summary>
        /// Specify the color of even list elements when in the dark skin.
        /// </summary>
        [TabGroup("Collections"), ShowInInspector]
        [PropertyTooltip("Specify the color of even list elements when in the dark skin.")]
        public Color ListItemColorEvenDarkSkin
        {
            get
            {
                return listItemColorEvenDarkSkinPref.Value;
            }

            set
            {
                if (listItemColorEvenDarkSkinPref.Value != value)
                {
                    listItemColorEvenDarkSkinPref.Value = value;

                    if (EditorGUIUtility.isProSkin)
                    {
                        SirenixGUIStyles.ListItemColorEven = value;
                    }
                }
            }
        }

        /// <summary>
        /// Specify the color of odd list elements when in the dark skin.
        /// </summary>
        [TabGroup("Collections"), ShowInInspector]
        [PropertyTooltip("Specify the color of odd list elements when in the dark skin.")]
        public Color ListItemColorOddDarkSkin
        {
            get
            {
                return listItemColorOddDarkSkinPref.Value;
            }

            set
            {
                if (listItemColorOddDarkSkinPref.Value != value)
                {
                    listItemColorOddDarkSkinPref.Value = value;

                    if (EditorGUIUtility.isProSkin)
                    {
                        SirenixGUIStyles.ListItemColorOdd = value;
                    }
                }
            }
        }

        /// <summary>
        /// Specify the color of even list elements when in the light skin.
        /// </summary>
        [TabGroup("Collections"), ShowInInspector]
        [PropertyTooltip("Specify the color of even list elements when in the light skin.")]
        public Color ListItemColorEvenLightSkin
        {
            get
            {
                return listItemColorEvenLightSkinPref.Value;
            }

            set
            {
                if (listItemColorEvenLightSkinPref.Value != value)
                {
                    listItemColorEvenLightSkinPref.Value = value;

                    if (!EditorGUIUtility.isProSkin)
                    {
                        SirenixGUIStyles.ListItemColorEven = value;
                    }
                }
            }
        }

        /// <summary>
        /// Specify the color of odd list elements when in the light skin.
        /// </summary>
        [TabGroup("Collections"), ShowInInspector]
        [PropertyTooltip("Specify the color of odd list elements when in the light skin.")]
        public Color ListItemColorOddLightSkin
        {
            get
            {
                return listItemColorOddLightSkinPref.Value;
            }

            set
            {
                if (listItemColorOddLightSkinPref.Value != value)
                {
                    listItemColorOddLightSkinPref.Value = value;

                    if (!EditorGUIUtility.isProSkin)
                    {
                        SirenixGUIStyles.ListItemColorOdd = value;
                    }
                }
            }
        }

        private class ColorPref
        {
            private Color? color;
            private Color defaultColor;
            private string key;

            public ColorPref(string key, Color defaultValue)
            {
                this.key = key;
                this.defaultColor = defaultValue;
            }

            public Color Value
            {
                get
                {
                    if (!this.color.HasValue)
                    {
                        var str = EditorPrefs.GetString(this.key, ColorToString(this.defaultColor));
                        Color parsed;

                        if (TryStringToColor(str, out parsed))
                        {
                            this.color = parsed;
                        }
                        else
                        {
                            this.color = this.defaultColor;
                        }
                    }

                    return this.color.Value;
                }

                set
                {
                    if (!this.color.HasValue || this.color.Value != value)
                    {

                        EditorPrefs.SetString(this.key, ColorToString(value));
                    }

                    this.color = value;
                }
            }

            public void Reset()
            {
                if (EditorPrefs.HasKey(this.key))
                {
                    EditorPrefs.DeleteKey(this.key);
                    this.color = null;
                }
            }

            private static unsafe string ColorToString(Color color)
            {
                Color32 col = color;

                byte[] bytes = new byte[4];

                fixed (byte* ptr = bytes)
                {
                    *((Color32*)ptr) = col;
                }

                return ProperBitConverter.BytesToHexString(bytes);
            }

            private static unsafe bool TryStringToColor(string data, out Color color)
            {
                try
                {
                    var bytes = ProperBitConverter.HexStringToBytes(data);

                    if (bytes.Length != 4)
                    {
                        color = default(Color);
                        return false;
                    }

                    fixed (byte* ptr = bytes)
                    {
                        Color32 col = *((Color32*)ptr);
                        color = col;
                        return true;
                    }
                }
                catch
                {
                    color = default(Color);
                    return false;
                }
            }
        }

        [TabGroup("Collections"), ShowInInspector]
        [NonSerialized, PropertyOrder(20)]
        private List<int> exampleList = new List<int>();

        private void ResizeExampleList()
        {
            this.exampleList = Enumerable.Range(0, Math.Max(10, (int)(this.NumberOfItemsPrPage * Mathf.PI))).ToList();
        }

#endregion

#region ObjectFields

        /// <summary>
        /// Gets or sets the default size of the preview object field.
        /// </summary>
        [TabGroup("Object Fields"), ShowInInspector]
        public float SquareUnityObjectFieldHeight
        {
            get
            {
                if (this.squareUnityObjectFieldHeight == null)
                {
                    this.squareUnityObjectFieldHeight = EditorPrefs.GetFloat("GeneralDrawerConfig.squareUnityObjectFieldHeight", 50);
                }

                return this.squareUnityObjectFieldHeight.Value;
            }
            set
            {
                this.squareUnityObjectFieldHeight = value;
                EditorPrefs.SetFloat("GeneralDrawerConfig.squareUnityObjectFieldHeight", value);
            }
        }

        /// <summary>
        /// Gets or sets the default alignment of the preview object field.
        /// </summary>
        [TabGroup("Object Fields"), ShowInInspector]
        [EnumToggleButtons]
        public ObjectFieldAlignment SquareUnityObjectAlignment
        {
            get
            {
                if (this.squareUnityObjectAlignment == null)
                {
                    this.squareUnityObjectAlignment = (ObjectFieldAlignment)EditorPrefs.GetInt("GeneralDrawerConfig.squareUnityObjectAlignment", (int)ObjectFieldAlignment.Right);
                }

                return this.squareUnityObjectAlignment.Value;
            }
            set
            {
                this.squareUnityObjectAlignment = value;
                EditorPrefs.SetInt("GeneralDrawerConfig.squareUnityObjectAlignment", (int)value);
            }
        }

        /// <summary>
        /// Gets or sets which types should be drawn by default by the preview object field.
        /// </summary>
        [LabelText("Enable Globally For")]
        [TabGroup("Object Fields"), ShowInInspector]
        public UnityObjectType SquareUnityObjectEnableFor
        {
            get
            {
                if (this.squareUnityObjectEnableFor == null)
                {
                    this.squareUnityObjectEnableFor = (UnityObjectType)EditorPrefs.GetInt("GeneralDrawerConfig.squareUnityObjectEnableFor", 0);
                }

                return this.squareUnityObjectEnableFor.Value;
            }
            set
            {
                this.squareUnityObjectEnableFor = value;
                EditorPrefs.SetInt("GeneralDrawerConfig.squareUnityObjectEnableFor", (int)value);
            }
        }

        [PreviewField, TabGroup("Object Fields"), ShowInInspector]
        private UnityEngine.Object ExampleObject { get; set; }

        [SerializeField]
        [PropertyOrder(9)]
        [TabGroup("Object Fields")]
        [Title("Backwards Compatibility")]
        public bool useOldUnityObjectField;

        [SerializeField]
        [PropertyOrder(9)]
        [TabGroup("Object Fields")]
        public bool useOldUnityPreviewField;
        
        [OnInspectorGUI]
        [PropertyOrder(999)]
        private void DrawFlexibleSpace()
        {
            GUILayout.FlexibleSpace();
        }

        [Flags]
        public enum UnityObjectType
        {
            Textures = 1 << 1,
            Sprites = 1 << 2,
            Materials = 1 << 3,
            GameObjects = 1 << 4,
            Components = 1 << 5,
            Others = 1 << 6
        }

#endregion

#region Buttons
        [TabGroup("Buttons"), ShowInInspector, CustomValueDrawer("DrawButtonHeight")]
        [OnValueChanged("@$property.Parent.Children[\"#Preview\"].RefreshSetup()")]
        public int ButtonHeight
        {
            get
            {
                if (this.buttonHeight == null)
                {
                    this.buttonHeight = EditorPrefs.GetInt("GeneralDrawerConfig.buttonHeight", 0);
                }

                return this.buttonHeight.Value;
            }
            set
            {
                this.buttonHeight = value;
                EditorPrefs.SetInt("GeneralDrawerConfig.buttonHeight", value);
            }
        }

        private int DrawButtonHeight(GUIContent label, int height, Func<GUIContent, bool> callNextDrawer)
        {
            EditorGUILayout.BeginHorizontal();
            height = (int)EnumSelector<ButtonSizes>.DrawEnumField(label, (ButtonSizes)height);
            height = SirenixEditorFields.DelayedIntField(height, GUILayout.Width(50f));
            EditorGUILayout.EndHorizontal();
            return height;
        }

        [TabGroup("Buttons"), ShowInInspector]
        [OnValueChanged("@$property.Parent.Children[\"#Preview\"].RefreshSetup()")]
        [EnumToggleButtons]
        public IconAlignment ButtonIconAlignment
        {
            get
            {
                if (this.buttonIconAlignment == null)
                {
                    this.buttonIconAlignment = (IconAlignment)EditorPrefs.GetInt("GeneralDrawerConfig.buttonIconAlignment", 0);
                }

                return this.buttonIconAlignment.Value;
            }
            set
            {
                this.buttonIconAlignment = value;
                EditorPrefs.SetInt("GeneralDrawerConfig.buttonIconAlignment", (int)value);
            }
        }

        [TabGroup("Buttons"), ShowInInspector]
        [OnValueChanged("@$property.Parent.Children[\"#Preview\"].RefreshSetup()")]
        public bool StretchButtons
        {
            get
            {
                if (this.stretchButtons == null)
                {
                    this.stretchButtons = EditorPrefs.GetBool("GeneralDrawerConfig.stretchButtons", true);
                }

                return this.stretchButtons.Value;
            }
            set
            {
                this.stretchButtons = value;
                EditorPrefs.SetBool("GeneralDrawerConfig.stretchButtons", value);
            }
        }


        [TabGroup("Buttons"), ShowInInspector, PropertyRange(0f, 1f)]
        [EnableIf("@!StretchButtons")]
        [OnValueChanged("@$property.Parent.Children[\"#Preview\"].RefreshSetup()")]
        public float ButtonAlignment
        {
            get
            {
                if (this.buttonAlignment == null)
                    this.buttonAlignment = EditorPrefs.GetFloat("GeneralDrawerConfig.buttonAlignment", 0.5f);

                return this.buttonAlignment.Value;
            }
            set
            {
                this.buttonAlignment = value;
                EditorPrefs.SetFloat("GeneralDrawerConfig.buttonAlignment", value);
            }
        }

        [TitleGroup("_DefaultTabGroup/Buttons/Preview")]
        [Button("Example Button With Icon", Icon = SdfIconType.CircleFill)]
        private void ExampleButton_WithIcon() { }

        [TitleGroup("_DefaultTabGroup/Buttons/Preview")]
        [Button("Example Button Without Icon")]
        private void ExampleButton_WithoutIcon() { }

#endregion

#region TypeSelector

        [SerializeField]
        [TabGroup("Type Selector")]
        [Title("Backwards Compatibility")]
        [PropertyOrder(10)]
        public bool useOldTypeSelector;

        [SerializeField]
        [TabGroup("Type Selector")]
        [Title("Backwards Compatibility")]
        [PropertyOrder(10)]
        public bool useNewObjectSelector = true;
        
        [SerializeField]
        [TabGroup("Type Selector")]
        [VerticalGroup("_DefaultTabGroup/Type Selector/GeneralGroup")]
        [Title("General")]
        [LabelText("Show '<none>' Item In The Selector")]
        [PropertyOrder(10)]
        public bool showNoneItem = true;
        
        [SerializeField]
        [VerticalGroup("_DefaultTabGroup/Type Selector/GeneralGroup")]
        [PropertyOrder(10)]
        public bool showCategoriesByDefault;

        [SerializeField]
        [VerticalGroup("_DefaultTabGroup/Type Selector/GeneralGroup")]
        [PropertyOrder(10)]
        public bool preferNamespacesOverAssemblyCategories = true;

        [VerticalGroup("_DefaultTabGroup/Type Selector/GeneralGroup")]
        [Button("Customize", ButtonSizes.Large)]
        [PropertyOrder(10)]
        public void ShowTypeRegistryUserConfig() => EditorWindow.GetWindow<TypeRegistryUserConfigWindow>();

        [TabGroup("Polymorphic Fields")]
        [Title("Backwards Compatibility")]
        [PropertyOrder(10)]
        public bool useOldPolymorphicField;
        
        [SerializeField]
        [TabGroup("Polymorphic Fields")]
        [Title("General")]
        [PropertyOrder(12)]
        public bool showBaseType = true;

        [SerializeField]
        [TabGroup("Polymorphic Fields")]
        [EnumToggleButtons]
        [LabelText("How To Handle Non Default Constructors")]
        [PropertyOrder(12)]
        public NonDefaultConstructorPreference nonDefaultConstructorPreference;
        
#endregion

        /// <summary>
        /// Resets all settings to default.
        /// </summary>
        [Button(ButtonSizes.Large), PropertyOrder(1000)]
        public void ResetToDefault()
        {
            if (EditorPrefs.HasKey("GeneralDrawerConfig.ShowMonoScriptInEditor")) EditorPrefs.DeleteKey("GeneralDrawerConfig.ShowMonoScriptInEditor");
            if (EditorPrefs.HasKey("GeneralDrawerConfig.ShowPrefabModificationsDisabledMessage")) EditorPrefs.DeleteKey("GeneralDrawerConfig.ShowPrefabModificationsDisabledMessage");
            if (EditorPrefs.HasKey("GeneralDrawerConfig.MaxRecursiveDrawDepth")) EditorPrefs.DeleteKey("GeneralDrawerConfig.MaxRecursiveDrawDepth");
            if (EditorPrefs.HasKey("GeneralDrawerConfig.MarkObjectsDirtyOnButtonClick")) EditorPrefs.DeleteKey("GeneralDrawerConfig.MarkObjectsDirtyOnButtonClick");
            if (EditorPrefs.HasKey("GeneralDrawerConfig.QuaternionDrawMode")) EditorPrefs.DeleteKey("GeneralDrawerConfig.QuaternionDrawMode");
            if (EditorPrefs.HasKey("GeneralDrawerConfig.UseImprovedEnumDropDown")) EditorPrefs.DeleteKey("GeneralDrawerConfig.UseImprovedEnumDropDown");
            if (EditorPrefs.HasKey("GeneralDrawerConfig.DrawEnumTypeTitle")) EditorPrefs.DeleteKey("GeneralDrawerConfig.DrawEnumTypeTitle");
            if (EditorPrefs.HasKey("GeneralDrawerConfig.HideFoldoutWhileEmpty")) EditorPrefs.DeleteKey("GeneralDrawerConfig.HideFoldoutWhileEmpty");
            if (EditorPrefs.HasKey("GeneralDrawerConfig.HidePagingWhileCollapsed")) EditorPrefs.DeleteKey("GeneralDrawerConfig.HidePagingWhileCollapsed");
            if (EditorPrefs.HasKey("GeneralDrawerConfig.HidePagingWhileOnlyOnePage")) EditorPrefs.DeleteKey("GeneralDrawerConfig.HidePagingWhileOnlyOnePage");
            if (EditorPrefs.HasKey("GeneralDrawerConfig.NumberOfItemsPrPage")) EditorPrefs.DeleteKey("GeneralDrawerConfig.NumberOfItemsPrPage");
            if (EditorPrefs.HasKey("GeneralDrawerConfig.OpenListsByDefault")) EditorPrefs.DeleteKey("GeneralDrawerConfig.OpenListsByDefault");
            if (EditorPrefs.HasKey("GeneralDrawerConfig.ShowExpandButton")) EditorPrefs.DeleteKey("GeneralDrawerConfig.ShowExpandButton");
            if (EditorPrefs.HasKey("GeneralDrawerConfig.ShowItemCount")) EditorPrefs.DeleteKey("GeneralDrawerConfig.ShowItemCount");
            if (EditorPrefs.HasKey("GeneralDrawerConfig.ShowIndexLabels")) EditorPrefs.DeleteKey("GeneralDrawerConfig.ShowIndexLabels");
            if (EditorPrefs.HasKey("GeneralDrawerConfig.squareUnityObjectFieldHeight")) EditorPrefs.DeleteKey("GeneralDrawerConfig.squareUnityObjectFieldHeight");
            if (EditorPrefs.HasKey("GeneralDrawerConfig.squareUnityObjectAlignment")) EditorPrefs.DeleteKey("GeneralDrawerConfig.squareUnityObjectAlignment");
            if (EditorPrefs.HasKey("GeneralDrawerConfig.squareUnityObjectEnableFor")) EditorPrefs.DeleteKey("GeneralDrawerConfig.squareUnityObjectEnableFor");
            if (EditorPrefs.HasKey("GeneralDrawerConfig.ShowPagingInTables")) EditorPrefs.DeleteKey("GeneralDrawerConfig.ShowPagingInTables");
            if (EditorPrefs.HasKey("GeneralDrawerConfig.PrecomputeTypeMatching")) EditorPrefs.DeleteKey("GeneralDrawerConfig.PrecomputeTypeMatching");
            if (EditorPrefs.HasKey("GeneralDrawerConfig.ShowPrefabModifiedValueBar")) EditorPrefs.DeleteKey("GeneralDrawerConfig.ShowPrefabModifiedValueBar");

            if (EditorPrefs.HasKey("GeneralDrawerConfig.buttonHeight")) EditorPrefs.DeleteKey("GeneralDrawerConfig.buttonHeight");
            if (EditorPrefs.HasKey("GeneralDrawerConfig.buttonIconAlignment")) EditorPrefs.DeleteKey("GeneralDrawerConfig.buttonIconAlignment");
            if (EditorPrefs.HasKey("GeneralDrawerConfig.buttonAlignment")) EditorPrefs.DeleteKey("GeneralDrawerConfig.buttonAlignment");
            if (EditorPrefs.HasKey("GeneralDrawerConfig.stretchButtons")) EditorPrefs.DeleteKey("GeneralDrawerConfig.stretchButtons");

            this.useOldUnityObjectField = false;
            this.useOldUnityPreviewField = false;
            
            this.useOldTypeSelector = false;
            this.useNewObjectSelector = true;
            this.useOldPolymorphicField = false; 
            this.showNoneItem = true;
            this.showCategoriesByDefault = false;
            this.preferNamespacesOverAssemblyCategories = true;
            this.showBaseType = true;
            this.nonDefaultConstructorPreference = NonDefaultConstructorPreference.Exclude;

            // Reset colors and correct dependent styles. (Gross, but we've no choice.)
            {
                listItemColorEvenDarkSkinPref.Reset();
                listItemColorEvenLightSkinPref.Reset();
                listItemColorOddDarkSkinPref.Reset();
                listItemColorOddLightSkinPref.Reset();

                if (EditorGUIUtility.isProSkin)
                {
                    SirenixGUIStyles.ListItemColorEven = this.ListItemColorEvenDarkSkin;
                    SirenixGUIStyles.ListItemColorOdd = this.ListItemColorOddDarkSkin;
                }
                else
                {
                    SirenixGUIStyles.ListItemColorEven = this.ListItemColorEvenLightSkin;
                    SirenixGUIStyles.ListItemColorOdd = this.ListItemColorOddLightSkin;
                }
            }

            this.showMonoScriptInEditor = null;
            this.hideFoldoutWhileEmpty = null;
            this.openListsByDefault = null;
            this.showItemCount = null;
            this.numberOfItemsPerPage = null;
            this.hidePagingWhileCollapsed = null;
            this.hidePagingWhileOnlyOnePage = null;
            this.showExpandButton = null;
            this.quaternionDrawMode = null;
            this.showPrefabModificationsDisabledMessage = null;
            this.maxRecursiveDrawDepth = null;
            this.squareUnityObjectFieldHeight = null;
            this.squareUnityObjectAlignment = null;
            this.showIndexLabels = null;
            this.useNewImprovedEnumDropdown = null;
            this.drawEnumTypeTitle = null;
            this.showPagingInTables = null;
            this.precomputeTypeMatching = null;
            this.showPrefabModifiedValueBar = null;
            this.buttonHeight = null;
            this.buttonIconAlignment = null;
            this.buttonAlignment = null;
            this.stretchButtons = null;

            EditorPrefs.DeleteKey("SirenixEditorGUI.DefaultFadeGroupDuration");
            EditorPrefs.DeleteKey("SirenixEditorGUI.TabPageSlideAnimationDuration");
            EditorPrefs.DeleteKey("SirenixEditorGUI.ShakingAnimationDuration");
            EditorPrefs.DeleteKey("SirenixEditorGUI.ExpandFoldoutByDefault");

            SirenixEditorGUI.DefaultFadeGroupDuration = 0.13f;
            SirenixEditorGUI.TabPageSlideAnimationDuration = 0.13f;
            SirenixEditorGUI.ShakingAnimationDuration = 0.5f;
            SirenixEditorGUI.ExpandFoldoutByDefault = false;
        }

        private void GotoTargetTabName(InspectorProperty property)
        {
            if (string.IsNullOrEmpty(this.TargetTabName))
            {
                return;
            }

            property.State.Set(TabGroupAttributeDrawer.CurrentTabNameKey, this.TargetTabName);
            
            this.TargetTabName = null;
        }
    }
}
#pragma warning restore 0414
#endif