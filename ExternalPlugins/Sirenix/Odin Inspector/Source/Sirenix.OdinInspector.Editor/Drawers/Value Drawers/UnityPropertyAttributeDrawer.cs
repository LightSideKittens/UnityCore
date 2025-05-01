//-----------------------------------------------------------------------
// <copyright file="UnityPropertyAttributeDrawer.cs" company="Sirenix ApS">
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

    using Sirenix.OdinInspector.Editor.Internal.UIToolkitIntegration;
    using System;
    using System.Reflection;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using Utilities;
    using Utilities.Editor;

    /// <summary>
    /// Unity property attribute drawer.
    /// </summary>
    [DrawerPriority(0, 0, 999.5), OdinDontRegisterAttribute]
    public sealed class UnityPropertyAttributeDrawer<TDrawer, TAttribute, TAttributeConstraint> : OdinAttributeDrawer<TAttribute>, IDisposable, IUnityPropertyFieldDrawer
        where TAttribute : TAttributeConstraint
        where TAttributeConstraint : PropertyAttribute
        where TDrawer : PropertyDrawer, new()
    {
        private static readonly FieldInfo InternalAttributeFieldInfo = typeof(TDrawer).GetField("m_Attribute", Flags.InstanceAnyVisibility);
        private static readonly FieldInfo InternalFieldInfoFieldInfo = typeof(TDrawer).GetField("m_FieldInfo", Flags.InstanceAnyVisibility);
        private static MethodInfo createPropertyGUIMethod;

        private static readonly ValueSetter<TDrawer, Attribute> SetAttribute;
        private static readonly ValueSetter<TDrawer, FieldInfo> SetFieldInfo;

        private TDrawer drawer;
        private object propertyHandler;
        private OdinImGuiElement element;

        public bool WillDrawPropertyField => createPropertyGUIMethod != null || this.propertyHandler != null;

        /// <summary>
        /// Initializes the drawer.
        /// </summary>
        public UnityPropertyAttributeDrawer()
        {
            this.drawer = new TDrawer();

            if (UnityPropertyHandlerUtility.IsAvailable)
            {
                this.propertyHandler = UnityPropertyHandlerUtility.CreatePropertyHandler(this.drawer);
            }
        }

        static UnityPropertyAttributeDrawer()
        {
            if (InternalAttributeFieldInfo == null)
            {
                Debug.LogError("Could not find the internal Unity field 'PropertyDrawer.m_Attribute'; UnityPropertyAttributeDrawer alias '" + typeof(UnityPropertyAttributeDrawer<TDrawer, TAttribute, TAttributeConstraint>).GetNiceName() + "' has been disabled.");
            }
            else
            {
                SetAttribute = EmitUtilities.CreateInstanceFieldSetter<TDrawer, Attribute>(InternalAttributeFieldInfo);
            }

            if (InternalFieldInfoFieldInfo == null)
            {
                Debug.LogError("Could not find the internal Unity field 'PropertyDrawer.m_FieldInfo'; UnityPropertyAttributeDrawer alias '" + typeof(UnityPropertyAttributeDrawer<TDrawer, TAttribute, TAttributeConstraint>).GetNiceName() + "' has been disabled.");
            }
            else
            {
                SetFieldInfo = EmitUtilities.CreateInstanceFieldSetter<TDrawer, FieldInfo>(InternalFieldInfoFieldInfo);
            }

			var method = typeof(TDrawer).GetMethod("CreatePropertyGUI", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(SerializedProperty) }, null);

			if (method.DeclaringType != typeof(PropertyDrawer))
			{
				createPropertyGUIMethod = method;
			}
		}

        protected override void Initialize()
        {
            if (this.Property.ChildResolver is ICollectionResolver)
            {
                // Don't draw Unity attribute drawers for collections, only for their elements
                this.SkipWhenDrawing = true;
            }
        }

        /// <summary>
        /// Draws the proprety.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var property = this.Property;
            var entry = property.ValueEntry;

            if (SetAttribute == null || SetFieldInfo == null)
            {
                SirenixEditorGUI.ErrorMessageBox("Could not find the internal Unity fields 'PropertyDrawer.m_Attribute' or 'PropertyDrawer.m_FieldInfo'; UnityPropertyAttributeDrawer alias '" + typeof(UnityPropertyAttributeDrawer<TDrawer, TAttribute, TAttributeConstraint>).GetNiceName() + "' has been disabled.");
                return;
            }

            if (entry == null)
            {
                SirenixEditorGUI.ErrorMessageBox("Cannot put the attribute '" + typeof(TAttribute) + "' on a property of type '" + property.Info.PropertyType + "'.");
                return;
            }

            FieldInfo fieldInfo;
            SerializedProperty unityProperty = property.Tree.GetUnityPropertyForPath(property.Path, out fieldInfo);

            if (unityProperty == null)
            {
                if (UnityVersion.IsVersionOrGreater(2017, 1))
                {
                    this.CallNextDrawer(label);
                }
                else
                {
                    SirenixEditorGUI.ErrorMessageBox("Could not get a Unity SerializedProperty for the property '" + property.NiceName + "' of type '" + entry.TypeOfValue.GetNiceName() + "' at path '" + entry.Property.Path + "'.");
                }

                return;
            }

            SetFieldInfo(ref this.drawer, fieldInfo);
            SetAttribute(ref this.drawer, this.Attribute);

            label = label ?? GUIContent.none;

            if (unityProperty.serializedObject.targetObject is EmittedScriptableObject)
            {
                var targetObjects = unityProperty.serializedObject.targetObjects;

                for (int i = 0; i < targetObjects.Length; i++)
                {
                    EmittedScriptableObject target = (EmittedScriptableObject)targetObjects[i];
                    target.SetWeakValue(entry.WeakValues[i]);
                }

                unityProperty.serializedObject.Update();
                unityProperty = unityProperty.serializedObject.FindProperty(unityProperty.propertyPath);
            }

            if (GeneralDrawerConfig.Instance.EnableUIToolkitSupport && createPropertyGUIMethod != null && this.element == null)
            {
                var propField = new PropertyField(unityProperty);

				ImguiElementUtils.RegisterSerializedPropertyChangeEventCallback(propField, prop =>
				{
                    if (prop.serializedObject.targetObject is EmittedScriptableObject)
                    {
                        ApplyValueWeak(this.Property.ValueEntry, prop);
                    }
				});

                this.element = new OdinImGuiElement(propField, unityProperty);
                this.element.Bind(unityProperty.serializedObject);
            }

            if (GeneralDrawerConfig.Instance.EnableUIToolkitSupport && this.element != null)
            {
                ImguiElementUtils.EmbedVisualElementAndDrawItHere(this.element);
            }
            else
            {
                float height;
                if (this.propertyHandler != null)
                {
                    height = UnityPropertyHandlerUtility.PropertyHandlerGetHeight(this.propertyHandler, unityProperty.Copy(), label, false);
                }
                else
                {
                    height = this.drawer.GetPropertyHeight(unityProperty.Copy(), label);
                }
            
                Rect position = EditorGUILayout.GetControlRect(false, height);

                EditorGUI.BeginChangeCheck();

                if (this.propertyHandler != null)
                {
                    UnityPropertyHandlerUtility.PropertyHandlerOnGUI(this.propertyHandler, position, unityProperty, label, false);
                }
                else
                {
                    this.drawer.OnGUI(position, unityProperty, label);
                }

                var changed = EditorGUI.EndChangeCheck();

                if (unityProperty.serializedObject.targetObject is EmittedScriptableObject)
                {
                    if (unityProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo() || changed)
                    {
                        ApplyValueWeak(entry, unityProperty);
                    }
                }
                else if (changed)
                {
                    this.Property.Tree.DelayActionUntilRepaint(() =>
                    {
                        var baseEntry = this.Property.BaseValueEntry;

                        for (int i = 0; i < baseEntry.ValueCount; i++)
                        {
                            (baseEntry as PropertyValueEntry).TriggerOnValueChanged(i);
                        }
                    });
                }
            }

            if (label == GUIContent.none && label.text != "")
            {
                label.text = "";
            }
        }

        private static void ApplyValueWeak(IPropertyValueEntry entry, SerializedProperty unityProperty)
        {
            if (!entry.IsEditable) return;

            var targetObjects = unityProperty.serializedObject.targetObjects;

            for (int i = 0; i < targetObjects.Length; i++)
            {
                EmittedScriptableObject target = (EmittedScriptableObject)targetObjects[i];
                entry.WeakValues[i] = target.GetWeakValue();
            }

            entry.WeakValues.ForceMarkDirty();
        }

        public void Dispose()
        {
            if (this.element != null)
            {
                this.element.Unbind();
                this.element.RemoveFromHierarchy();
                this.element = null;
            }
        }
    }
}
#endif