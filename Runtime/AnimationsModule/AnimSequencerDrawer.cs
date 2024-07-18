#if UNITY_EDITOR
using System;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LSCore.AnimationsModule
{
    public class AnimSequencerDrawer : OdinValueDrawer<AnimSequencer>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (ShouldIgnoreDrawer(Property))
            {
                CallNextDrawer(label);
                return;
            }

            var rect = EditorGUILayout.GetControlRect();
            rect.TakeFromLeft(EditorGUI.indentLevel * 15);
            
            if (GUI.Button(rect, $"Edit {label}"))
            {
                var editor = EditorWindow.GetWindow<AnimSequencerEditor>();
                editor.titleContent.text = $"{Property.NiceName} in {Property.SerializationRoot.ValueEntry.WeakSmartValue}";
                editor.sequencer = ValueEntry.SmartValue;
                editor.rootObject = Property.SerializationRoot.ValueEntry.WeakSmartValue as Object;
                editor.Show();
            }
        }
        
        private static bool ShouldIgnoreDrawer(InspectorProperty property)
        {
            return property.GetAttribute<IgnoreOdinValueDrawerAttribute>() != null;
        }
    }
    
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class IgnoreOdinValueDrawerAttribute : Attribute { }
    
    public class AnimSequencerEditor : OdinEditorWindow
    {
        [IgnoreOdinValueDrawer] public AnimSequencer sequencer;
        [NonSerialized] public Object rootObject;
        private bool isDirty;
        
        protected override void OnImGUI()
        {
            if (isDirty)
            {
                EditorUtility.SetDirty(rootObject);
                isDirty = false;
            }
            
            if (rootObject == null)
            {
                Close();
                return;
            }

            GUI.enabled = false;
            EditorGUILayout.ObjectField(rootObject, typeof(Object), true);
            GUI.enabled = true;
            
            EditorGUI.BeginChangeCheck();
            base.OnImGUI();
            if (EditorGUI.EndChangeCheck())
            {
                isDirty = true;
                EditorUtility.SetDirty(rootObject);
                Repaint();
            }
        }
    }
}
#endif