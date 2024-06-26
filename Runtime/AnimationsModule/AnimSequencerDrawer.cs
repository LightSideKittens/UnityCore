#if UNITY_EDITOR
using System;
using Sirenix.OdinInspector.Editor;
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

            if (GUILayout.Button($"Edit {label}"))
            {
                var editor = EditorWindow.GetWindow<AnimSequencerEditor>();
                editor.titleContent.text = $"{Property.NiceName} in {Property.SerializationRoot.ValueEntry.WeakSmartValue}";
                editor.sequencer = ValueEntry.SmartValue;
                editor.selected = Selection.activeObject;
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
        [NonSerialized] public Object selected;

        protected override void OnImGUI()
        {
            if (selected == null)
            {
                Close();
                return;
            }

            GUI.enabled = false;
            EditorGUILayout.ObjectField(selected, typeof(Object), true);
            GUI.enabled = true;
            
            EditorGUI.BeginChangeCheck();
            base.OnImGUI();
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(selected);
            }
        }
    }
}
#endif