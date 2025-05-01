//-----------------------------------------------------------------------
// <copyright file="PreviewFieldAttributeDrawer.cs" company="Sirenix ApS">
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
    using Sirenix.OdinInspector.Editor.Internal;
    using UnityEditor;
    using UnityEngine;
    using ValueResolvers;

    /// <summary>
    /// Draws properties marked with <see cref="PreviewFieldAttribute"/> as a square ObjectField which renders a preview for UnityEngine.Object types.
    /// This object field also adds support for drag and drop, dragging an object to another square object field, swaps the values.
    /// If you hold down control while letting go it will replace the value, And you can control + click the object field to quickly delete the value it holds.
    /// </summary>

    [AllowGUIEnabledForReadonly]
    public sealed class PreviewFieldAttributeDrawer<T> : OdinAttributeDrawer<PreviewFieldAttribute, T>
        where T : UnityEngine.Object
    {
        private ValueResolver<UnityEngine.Object> previewResolver;

        private bool allowSceneObjects;

        protected override void Initialize()
        {
            this.previewResolver = ValueResolver.Get<UnityEngine.Object>(this.Property, this.Attribute.PreviewGetter);
            this.allowSceneObjects = InspectorPropertyInfoUtility.InspectorPropertySupportsAssigningSceneReferences(this.Property);
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (this.Attribute.PreviewGetterHasValue && this.previewResolver.HasError)
            {
                this.previewResolver.DrawError();
                this.CallNextDrawer(label);
                return;
            }

            EditorGUI.BeginChangeCheck();

            ObjectFieldAlignment alignment;

            if (this.Attribute.AlignmentHasValue)
            {
                alignment = (ObjectFieldAlignment)this.Attribute.Alignment;
            }
            else
            {
                alignment = GeneralDrawerConfig.Instance.SquareUnityObjectAlignment;
            }

            var previewHeight = this.Attribute.Height == 0
                ? GeneralDrawerConfig.Instance.SquareUnityObjectFieldHeight
                : this.Attribute.Height;

            Texture previewTexture;

            // The user provided a preview so try to get it.
            if (this.Attribute.PreviewGetterHasValue)
            {
                var resolvedPreview = this.previewResolver.GetValue();
                previewTexture = resolvedPreview == null ? null : GUIHelper.GetPreviewTexture(resolvedPreview);
            }
            // The user has not provided a preview so try to get one using the object's value.
            else
            {
                var value = this.ValueEntry.WeakSmartValue as UnityEngine.Object;
                previewTexture = value == null ? null : GUIHelper.GetPreviewTexture(value);
            }

            // NOTE: in the case that the preview texture returns the texture itself, we need to reset the filter mode -- e.g. a RenderTexture.
            var lastFilterMode = FilterMode.Bilinear;

            if (previewTexture != null)
            {
                lastFilterMode = previewTexture.filterMode;

                previewTexture.filterMode = this.Attribute.FilterMode;
            }

            try
            {
                if (GeneralDrawerConfig.Instance.useOldUnityPreviewField)
                {
                    this.ValueEntry.WeakSmartValue = OdinInternalEditorFields.UnityPreviewObjectField(label,
                                                                                                      this.ValueEntry.WeakSmartValue as UnityEngine.Object,
                                                                                                      previewTexture,
                                                                                                      this.ValueEntry.BaseValueType,
                                                                                                     this.allowSceneObjects,
                                                                                                      previewHeight,
                                                                                                      alignment);
                }
                else
                {
                    OdinInternalEditorFields.UnityPreviewObjectField(this.Property,
                                                                     OdinObjectSelectorIds.ODIN_DRAWER_FIELD,
                                                                     label,
                                                                     this.ValueEntry.WeakSmartValue as UnityEngine.Object,
                                                                     previewTexture,
                                                                     this.ValueEntry.BaseValueType,
                                                                     this.allowSceneObjects,
                                                                     previewHeight,
                                                                     alignment,
                                                                     this.Property);
                }
            } finally
            {
                if (previewTexture != null)
                {
                    previewTexture.filterMode = lastFilterMode;
                }
            }


            if (EditorGUI.EndChangeCheck())
            {
                this.ValueEntry.Values.ForceMarkDirty();
            }
        }
    }
}
#endif