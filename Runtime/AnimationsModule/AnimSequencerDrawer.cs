﻿#if UNITY_EDITOR
using System;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LSCore.AnimationsModule
{
    public class AnimSequencerDrawer : OdinValueDrawer<AnimSequencer>
    {
        public static bool drawDefault;
        
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (drawDefault)
            {
                drawDefault = false;
                CallNextDrawer(label);
                return;
            }

            var rect = EditorGUILayout.GetControlRect();
            rect.TakeFromLeft(EditorGUI.indentLevel * 15);
            
            if (GUI.Button(rect, $"Edit {label}"))
            {
                var editor = EditorWindow.GetWindow<AnimSequencerEditor>();
                editor.tree?.Dispose();
                editor.tree = null;
                editor.prop = null;
                editor.titleContent.text = $"{Property.NiceName} in {Property.SerializationRoot.ValueEntry.WeakSmartValue}";
                editor.rootObject = Property.SerializationRoot.ValueEntry.WeakSmartValue as Object;
                editor.path = Property.Path;
                editor.Show();
            }
        }
    }
    
    public class AnimSequencerEditor : OdinEditorWindow
    {
        public Object rootObject;
        public string path;
        public PropertyTree tree;
        public InspectorProperty prop;
        
        protected override void OnGUI() { }
        
        protected override void OnImGUI()
        {
            EditorGUILayout.ObjectField(rootObject, typeof(Object), true);
            if (tree == null)
            {
                tree = PropertyTree.Create(rootObject);
                prop = tree.GetPropertyAtPath(path);
                CompilationPipeline.compilationFinished += Dispose;
            }
            
            AnimSequencerDrawer.drawDefault = true;
            tree.BeginDraw(true);
            {
                foreach (var child in prop.Children)
                {
                    child.Draw();
                }
            }
            tree.EndDraw();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Dispose(null);
        }
        

        private void Dispose(object c)
        {
            CompilationPipeline.compilationFinished -= Dispose;
            tree?.Dispose();
            tree = null;
            prop = null;
        }
    }
    
}
#endif