//-----------------------------------------------------------------------
// <copyright file="AbstractTypeUnityPropertyDrawer.cs" company="Sirenix ApS">
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
    using UnityEngine.UIElements;
    using Utilities;
    using Utilities.Editor;

    /// <summary>
    /// Unity property drawer for abstract types.
    /// </summary>
    [DrawerPriority(0, 0, 0.4999), OdinDontRegisterAttribute]
    public sealed class AbstractTypeUnityPropertyDrawer<TDrawer, TDrawnType, T> : OdinValueDrawer<T>, IDisposable, IUnityPropertyFieldDrawer
		where TDrawer : UnityEditor.PropertyDrawer, new()
        where T : TDrawnType
    {
        private static readonly FieldInfo InternalFieldInfoFieldInfo = typeof(TDrawer).GetField("m_FieldInfo", Flags.InstanceAnyVisibility);
        private static readonly ValueSetter<TDrawer, FieldInfo> SetFieldInfo;

        private TDrawer drawer;
        private object propertyHandler;
        private OdinImGuiElement element;
        private static MethodInfo createPropertyGUIMethod;

        public bool WillDrawPropertyField => createPropertyGUIMethod != null || this.propertyHandler != null;

        /// <summary>
        /// Initializes the property drawer.
        /// </summary>
        public AbstractTypeUnityPropertyDrawer()
        {
            this.drawer = new TDrawer();

            if (UnityPropertyHandlerUtility.IsAvailable)
            {
                this.propertyHandler = UnityPropertyHandlerUtility.CreatePropertyHandler(this.drawer);
            }
        }

        static AbstractTypeUnityPropertyDrawer()
        {
            if (InternalFieldInfoFieldInfo == null)
            {
                Debug.LogError("Could not find the internal Unity field 'PropertyDrawer.m_FieldInfo'; UnityPropertyDrawer alias '" + typeof(AbstractTypeUnityPropertyDrawer<TDrawer, TDrawnType, T>).GetNiceName() + "' has been disabled.");
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

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            if (SetFieldInfo == null)
            {
                SirenixEditorGUI.ErrorMessageBox("Could not find the internal Unity field 'PropertyDrawer.m_FieldInfo'; UnityPropertyDrawer alias '" + typeof(AbstractTypeUnityPropertyDrawer<TDrawer, TDrawnType, T>).GetNiceName() + "' has been disabled.");
                return;
            }
            
            FieldInfo fieldInfo;
            SerializedProperty unityProperty = entry.Property.Tree.GetUnityPropertyForPath(entry.Property.Path, out fieldInfo);

            if (unityProperty == null)
            {
                SirenixEditorGUI.ErrorMessageBox("Could not get a Unity SerializedProperty for the property '" + entry.Property.NiceName + "' of type '" + entry.TypeOfValue.GetNiceName() + "' at path '" + entry.Property.Path + "'. Legacy Unity drawing compatibility is broken for this property; falling back to normal Odin drawing. Please report an issue on Odin's issue tracker with details.");
                this.CallNextDrawer(label);
                return;
            }

            label = label ?? GUIContent.none;

            SetFieldInfo(ref this.drawer, fieldInfo);

            if (unityProperty.serializedObject.targetObject is EmittedScriptableObject<T>)
            {
                var targetObjects = unityProperty.serializedObject.targetObjects;

                for (int i = 0; i < targetObjects.Length; i++)
                {
                    EmittedScriptableObject<T> target = (EmittedScriptableObject<T>)targetObjects[i];
                    target.SetValue(entry.Values[i]);
                }

                unityProperty.serializedObject.Update();
            }
            else if (unityProperty.serializedObject.targetObject is EmittedScriptableObject)
            {
                var targetObjects = unityProperty.serializedObject.targetObjects;

                for (int i = 0; i < targetObjects.Length; i++)
                {
                    EmittedScriptableObject target = (EmittedScriptableObject)targetObjects[i];
                    target.SetWeakValue(entry.Values[i]);
                }

                unityProperty.serializedObject.Update();
                unityProperty = unityProperty.serializedObject.FindProperty(unityProperty.propertyPath);
            }

            if (GeneralDrawerConfig.Instance.EnableUIToolkitSupport && createPropertyGUIMethod != null && this.element == null)
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

            if (GeneralDrawerConfig.Instance.EnableUIToolkitSupport && this.element != null)
            {
                ImguiElementUtils.EmbedVisualElementAndDrawItHere(this.element);
            }
            else
            {
                bool changed;

                try
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

                    changed = EditorGUI.EndChangeCheck();
                }
                finally
                {
                    if (label == GUIContent.none && label.text != "")
                    {
                        // If they messed with the label text (bad!) then we have to fix that.
                        label.text = "";
                    }
                }

                if (unityProperty.serializedObject.targetObject is EmittedScriptableObject<T>)
                {
                    if (unityProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo() || changed)
                    {
                        ApplyValueStrong(entry, unityProperty);
                    }
                }
                else if (unityProperty.serializedObject.targetObject is EmittedScriptableObject)
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

        private static void ApplyValueWeak(IPropertyValueEntry<T> entry, SerializedProperty unityProperty)
        {
            var targetObjects = unityProperty.serializedObject.targetObjects;

            for (int i = 0; i < targetObjects.Length; i++)
            {
                EmittedScriptableObject target = (EmittedScriptableObject)targetObjects[i];
                entry.Values[i] = (T)target.GetWeakValue();
            }

            entry.Values.ForceMarkDirty();
        }

        private static void ApplyValueStrong(IPropertyValueEntry<T> entry, SerializedProperty unityProperty)
        {
            var targetObjects = unityProperty.serializedObject.targetObjects;

            for (int i = 0; i < targetObjects.Length; i++)
            {
                EmittedScriptableObject<T> target = (EmittedScriptableObject<T>)targetObjects[i];
                entry.Values[i] = target.GetValue();
            }

            entry.Values.ForceMarkDirty();
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