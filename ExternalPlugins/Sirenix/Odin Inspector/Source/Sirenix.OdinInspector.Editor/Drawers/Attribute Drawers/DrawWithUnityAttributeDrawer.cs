//-----------------------------------------------------------------------
// <copyright file="DrawWithUnityAttributeDrawer.cs" company="Sirenix ApS">
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
    using System.Reflection;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using Utilities;
    using Utilities.Editor;

    /// <summary>
    /// Draws properties marked with <see cref="DrawWithUnityAttribute"/>.
    /// </summary>
    /// <seealso cref="RequireComponent"/>
    /// <seealso cref="OnInspectorGUIAttribute"/>
    /// <seealso cref="InlineEditorAttribute"/>
    /// <seealso cref="HideInInspector"/>
    [DrawerPriority(0, 0, 6000)]
    public class DrawWithUnityAttributeDrawer<T> : OdinAttributeDrawer<DrawWithUnityAttribute, T>, IUnityPropertyFieldDrawer
	{
        private OdinImGuiElement element;

        public bool WillDrawPropertyField => element != null;

        protected override void Initialize()
        {
            if (this.Attribute.PreferImGUI || !GeneralDrawerConfig.Instance.EnableUIToolkitSupport) return;

            var unityProperty = this.Property.Tree.GetUnityPropertyForPath(this.Property.Path, out _);

            if (unityProperty != null)
            {
                var propField = new PropertyField(unityProperty);

				ImguiElementUtils.RegisterSerializedPropertyChangeEventCallback(propField, prop =>
				{
                    if (this.ValueEntry.IsEditable && prop.serializedObject.targetObject is EmittedScriptableObject<T>)
                    {
                        var targetObjects = prop.serializedObject.targetObjects;

                        for (int i = 0; i < targetObjects.Length; i++)
                        {
                            EmittedScriptableObject<T> target = (EmittedScriptableObject<T>)targetObjects[i];
                            this.ValueEntry.Values[i] = target.GetValue();
                        }

                        this.ValueEntry.Values.ForceMarkDirty();
                    }
                });

                this.element = new OdinImGuiElement(propField, unityProperty);
                this.element.Bind(unityProperty.serializedObject);
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;

            FieldInfo fieldInfo;
            SerializedProperty unityProperty = entry.Property.Tree.GetUnityPropertyForPath(entry.Property.Path, out fieldInfo);

            if (unityProperty == null)
            {
                SirenixEditorGUI.ErrorMessageBox("Could not get a Unity SerializedProperty for the property '" + entry.Property.NiceName + "' of type '" + entry.TypeOfValue.GetNiceName() + "' at path '" + entry.Property.Path + "'.");
                return;
            }

            if (unityProperty.serializedObject.targetObject is EmittedScriptableObject<T>)
            {
                var targetObjects = unityProperty.serializedObject.targetObjects;

                for (int i = 0; i < targetObjects.Length; i++)
                {
                    EmittedScriptableObject<T> target = (EmittedScriptableObject<T>)targetObjects[i];
                    target.SetValue(entry.Values[i]);
                }

                unityProperty.serializedObject.Update();
                unityProperty = unityProperty.serializedObject.FindProperty(unityProperty.propertyPath);
            }

            if (GeneralDrawerConfig.Instance.EnableUIToolkitSupport && this.element != null)
            {
                ImguiElementUtils.EmbedVisualElementAndDrawItHere(this.element, label);
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(unityProperty, label ?? GUIContent.none, true);
                var changed = EditorGUI.EndChangeCheck();

                if (unityProperty.serializedObject.targetObject is EmittedScriptableObject<T>)
                {
                    unityProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    var targetObjects = unityProperty.serializedObject.targetObjects;

                    for (int i = 0; i < targetObjects.Length; i++)
                    {
                        EmittedScriptableObject<T> target = (EmittedScriptableObject<T>)targetObjects[i];
                        entry.Values[i] = target.GetValue();
                    }

                    if (changed)
                    {
                        entry.Values.ForceMarkDirty();
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