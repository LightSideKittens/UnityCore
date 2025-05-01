//-----------------------------------------------------------------------
// <copyright file="UnityObjectDrawer.cs" company="Sirenix ApS">
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

    using Utilities.Editor;
    using UnityEngine;
    using UnityEditor;
    using Sirenix.OdinInspector.Editor.Internal;

    /// <summary>
    /// Unity object drawer.
    /// </summary>
    [DrawerPriority(0, 0, 0.25)] // Set priority so that vanilla Unity CustomPropertyDrawers can draw UnityObject types by default
    public sealed class UnityObjectDrawer<T> : OdinValueDrawer<T>, IDefinesGenericMenuItems
        where T : UnityEngine.Object
    {
        private bool ShowBaseType
        {
            get
            {
                if (this.polymorphicDrawerSettings == null)
                {
                    return true;
                }

                return !this.polymorphicDrawerSettings.ShowBaseTypeIsSet || this.polymorphicDrawerSettings.ShowBaseType;
            }
        }

        private bool ReadOnlyIfNotNullReference => this.polymorphicDrawerSettings?.ReadOnlyIfNotNullReference ?? false;
        
        private bool drawAsPreview;

        private PolymorphicDrawerSettingsAttribute polymorphicDrawerSettings;

        private bool allowSceneObjects;

        protected override bool CanDrawValueProperty(InspectorProperty property)
        {
            return !property.IsTreeRoot;
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        protected override void Initialize()
        {
            this.drawAsPreview = false;
            var flags = GeneralDrawerConfig.Instance.SquareUnityObjectEnableFor;
            this.drawAsPreview = (int) flags != 0 && (
                                                         (flags & GeneralDrawerConfig.UnityObjectType.Components) != 0 &&
                                                         typeof(Component).IsAssignableFrom(typeof(T)) ||
                                                         (flags & GeneralDrawerConfig.UnityObjectType.GameObjects) != 0 &&
                                                         typeof(GameObject).IsAssignableFrom(typeof(T)) ||
                                                         (flags & GeneralDrawerConfig.UnityObjectType.Materials) != 0 &&
                                                         typeof(Material).IsAssignableFrom(typeof(T)) ||
                                                         (flags & GeneralDrawerConfig.UnityObjectType.Sprites) != 0 &&
                                                         typeof(Sprite).IsAssignableFrom(typeof(T)) ||
                                                         (flags & GeneralDrawerConfig.UnityObjectType.Textures) != 0 &&
                                                         typeof(Texture).IsAssignableFrom(typeof(T)));

            if (!this.drawAsPreview && (flags & GeneralDrawerConfig.UnityObjectType.Others) != 0)
            {
                bool isOther =
                    !typeof(Component).IsAssignableFrom(typeof(T)) &&
                    !typeof(GameObject).IsAssignableFrom(typeof(T)) &&
                    !typeof(Material).IsAssignableFrom(typeof(T)) &&
                    !typeof(Sprite).IsAssignableFrom(typeof(T)) &&
                    !typeof(Texture).IsAssignableFrom(typeof(T));

                if (isOther)
                {
                    this.drawAsPreview = true;
                }
            }

            this.polymorphicDrawerSettings = this.Property.GetAttribute<PolymorphicDrawerSettingsAttribute>();
            this.allowSceneObjects = InspectorPropertyInfoUtility.InspectorPropertySupportsAssigningSceneReferences(this.Property);
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;

            bool isPolymorphic = entry.BaseValueType == typeof(object) ||
                                 !typeof(UnityEngine.Object).IsAssignableFrom(entry.BaseValueType) ||
                                 entry.BaseValueType.IsInterface;

            if (!drawAsPreview)
            {
                if (isPolymorphic)
                {
                    Rect position = EditorGUILayout.GetControlRect();

                    bool isReadOnly = this.ReadOnlyIfNotNullReference && this.ValueEntry.WeakSmartValue != null;

                    if (GeneralDrawerConfig.Instance.useOldPolymorphicField)
                    {
                        if (label != null)
                        {
                            position = EditorGUI.PrefixLabel(position, label);
                        }

                        EditorGUI.BeginChangeCheck();
                        var prev = EditorGUI.showMixedValue;
                        if (this.ValueEntry.ValueState == PropertyValueState.ReferenceValueConflict)
                        {
                            EditorGUI.showMixedValue = true;
                        }

                        var newValue = OdinInternalEditorFields.PolymorphicObjectField(position, null, this.Property, this.allowSceneObjects, isReadOnly, this.ShowBaseType);

                        EditorGUI.showMixedValue = prev;

                        if (EditorGUI.EndChangeCheck())
                        {
                            this.ValueEntry.Property.Tree.DelayActionUntilRepaint(() =>
                            {
                                this.ValueEntry.WeakValues[0] = newValue;
                                for (int j = 1; j < this.ValueEntry.ValueCount; j++)
                                {
                                    // NOTE: "Sirenix.Serialization." is important for later Unity versions
                                    this.ValueEntry.WeakValues[j] = Sirenix.Serialization.SerializationUtility.CreateCopy(newValue);
                                }
                            });
                        }
                    }
                    else
                    {
                        OdinInternalEditorFields.PolymorphicObjectField(this.Property, OdinObjectSelectorIds.ODIN_DRAWER_FIELD,
                                                                        position, label, this.Property, this.allowSceneObjects, isReadOnly, this.ShowBaseType);
                    }
                }
                else
                {
                    if (GeneralDrawerConfig.Instance.useOldUnityObjectField)
                    {
                        this.ValueEntry.WeakSmartValue = OdinInternalEditorFields.UnityObjectField(EditorGUILayout.GetControlRect(),
                                                                                                   label,
                                                                                                   entry.WeakSmartValue as UnityEngine.Object,
                                                                                                   entry.BaseValueType,
                                                                                                   this.allowSceneObjects);
                    }
                    else
                    {
                        OdinInternalEditorFields.UnityObjectField(this.Property,
                                                                  OdinObjectSelectorIds.ODIN_DRAWER_FIELD,
                                                                  EditorGUILayout.GetControlRect(),
                                                                  label,
                                                                  entry.WeakSmartValue as UnityEngine.Object,
                                                                  entry.BaseValueType,
                                                                  this.allowSceneObjects,
                                                                  property: this.Property);
                    }
                }
            }
            else
            {
                if (GeneralDrawerConfig.Instance.useOldUnityPreviewField)
                {
                    entry.WeakSmartValue = OdinInternalEditorFields.UnityPreviewObjectField(label,
                                                                                            entry.WeakSmartValue as UnityEngine.Object,
                                                                                            entry.BaseValueType,
                                                                                            this.allowSceneObjects,
                                                                                            GeneralDrawerConfig.Instance.SquareUnityObjectFieldHeight,
                                                                                            GeneralDrawerConfig.Instance.SquareUnityObjectAlignment);
                }
                else
                {
                    OdinInternalEditorFields.UnityPreviewObjectField(this.Property,
                                                                     OdinObjectSelectorIds.ODIN_DRAWER_FIELD,
                                                                     label,
                                                                     entry.WeakSmartValue as UnityEngine.Object,
                                                                     entry.BaseValueType,
                                                                     this.allowSceneObjects,
                                                                     GeneralDrawerConfig.Instance.SquareUnityObjectFieldHeight,
                                                                     GeneralDrawerConfig.Instance.SquareUnityObjectAlignment,
                                                                     this.Property);
                }
            }
        }

        void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            var unityObj = property.ValueEntry.WeakSmartValue as UnityEngine.Object;
            if (unityObj)
            {
                genericMenu.AddItem(new GUIContent("Open in new inspector"), false, () =>
                {
                    GUIHelper.OpenInspectorWindow(unityObj);
                });
            }
            else
            {
                genericMenu.AddDisabledItem(new GUIContent("Open in new inspector"));
            }
        }
    }
}
#endif