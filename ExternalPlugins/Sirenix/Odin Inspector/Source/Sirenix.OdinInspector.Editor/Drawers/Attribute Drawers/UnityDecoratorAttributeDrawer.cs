//-----------------------------------------------------------------------
// <copyright file="UnityDecoratorAttributeDrawer.cs" company="Sirenix ApS">
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

    using Sirenix.OdinInspector.Editor.Internal.UIToolkitIntegration;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;
    using Utilities;


    /// <summary>
    /// Draws all Unity DecoratorDrawers within prepend attribute drawers within Odin.
    /// </summary>
    [DrawerPriority(0, 1, 0), OdinDontRegister]
    public sealed class UnityDecoratorAttributeDrawer<TDrawer, TAttribute, TAttributeConstraint> : OdinAttributeDrawer<TAttribute>, IDisposable
        where TDrawer : UnityEditor.DecoratorDrawer, new()
        where TAttribute : TAttributeConstraint
        where TAttributeConstraint : PropertyAttribute
    {
        private static readonly FieldInfo InternalAttributeFieldInfo = typeof(TDrawer).GetField("m_Attribute", Flags.InstanceAnyVisibility);
        private static readonly ValueSetter<TDrawer, Attribute> SetAttribute;

        private TDrawer drawer = new TDrawer();
        private bool hasCheckedForVisualElement;
        private OdinImGuiElement imguiElement;


        /// <summary>
        /// Initializes the <see cref="UnityDecoratorAttributeDrawer{TDrawer, TAttribute}"/> class.
        /// </summary>
        static UnityDecoratorAttributeDrawer()
        {
            if (InternalAttributeFieldInfo == null)
            {
                Debug.LogError("Could not find the internal Unity field 'DecoratorDrawer.m_Attribute'; UnityDecoratorDrawer alias '" + typeof(UnityDecoratorAttributeDrawer<TDrawer, TAttribute, TAttributeConstraint>).GetNiceName() + "' has been disabled.");
            }
            else
            {
                SetAttribute = EmitUtilities.CreateInstanceFieldSetter<TDrawer, Attribute>(InternalAttributeFieldInfo);
            }
        }

        public void Dispose()
        {
            if (this.imguiElement != null)
            {
                this.imguiElement.RemoveFromHierarchy();
                this.imguiElement = null;
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (this.Property.Parent != null && this.Property.Parent.ChildResolver is ICollectionResolver)
            {
                // Don't draw decorators for list elements
                this.CallNextDrawer(label);
                return;
            }

            if (SetAttribute == null)
            {
                SirenixEditorGUI.ErrorMessageBox("Could not find the internal Unity field 'DecoratorDrawer.m_Attribute'; UnityDecoratorDrawer alias '" + typeof(UnityDecoratorAttributeDrawer<TDrawer, TAttribute, TAttributeConstraint>).GetNiceName() + "' has been disabled.");
                return;
            }

            SetAttribute(ref this.drawer, this.Attribute);

            if (!hasCheckedForVisualElement && GeneralDrawerConfig.Instance.EnableUIToolkitSupport)
            {
                hasCheckedForVisualElement = true;

                if (DrawerUtilities.DecoratorDrawerCreatePropertyGUIMethod != null)
                {
                    var element = DrawerUtilities.DecoratorDrawerCreatePropertyGUIMethod.Invoke(this.drawer, null) as VisualElement;

                    if (element != null)
                    {
                        this.imguiElement = new OdinImGuiElement(element);
                    }
                }
            }

            bool willDrawPropertyField = false;

            // If any drawer is going to draw a property field, then we don't want to draw the decorator drawer
            // as the property field will include the decorator drawer itself.
            foreach (var drawer in this.Property.GetActiveDrawerChain())
            {
                if (drawer is IUnityPropertyFieldDrawer pDrawer && !drawer.SkipWhenDrawing && pDrawer.WillDrawPropertyField)
                {
                    willDrawPropertyField = true;
                    break;
                }
            }

            if (!willDrawPropertyField)
            {
                if (GeneralDrawerConfig.Instance.EnableUIToolkitSupport && this.imguiElement != null)
                {
                    ImguiElementUtils.EmbedVisualElementAndDrawItHere(this.imguiElement);
                }
                else
                {
                    float height = this.drawer.GetHeight();
                    var position = EditorGUILayout.GetControlRect(false, height);
                    this.drawer.OnGUI(position);
                }
            }

            this.CallNextDrawer(label);
        }
    }
}
#endif