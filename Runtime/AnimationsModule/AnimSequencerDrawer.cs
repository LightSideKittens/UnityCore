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
                editor.Show();
                editor.sequencer = ValueEntry.SmartValue;
                editor.selected = Selection.activeObject;
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
        
        protected override void OnGUI()
        {
            if (selected == null || Selection.activeObject != selected)
            {
                selected = null;
                Close();
                return;
            }

            EditorGUI.BeginChangeCheck();
            base.OnGUI();
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(selected);
            }
        }
    }
}
#endif