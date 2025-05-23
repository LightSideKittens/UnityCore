﻿#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace LSCore
{
    [CustomEditor(typeof(LSRawImage), true)]
    [CanEditMultipleObjects]
    public class LSRawImageEditor : RawImageEditor
    {
        LSRawImage image;
        SerializedProperty m_Texture;
        SerializedProperty preserveAspectRatio;
        SerializedProperty rotateId;

        protected override void OnEnable()
        {
            base.OnEnable();
            image = (LSRawImage)target;
            m_Texture = serializedObject.FindProperty("m_Texture");
            preserveAspectRatio = serializedObject.FindProperty("preserveAspectRatio");
            rotateId = serializedObject.FindProperty("rotateId");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Texture);

            AppearanceControlsGUI();
            RaycastControlsGUI();
            MaskableControlsGUI();
            SetShowNativeSize(m_Texture.objectReferenceValue != null, false);
            NativeSizeButtonGUI();

            serializedObject.ApplyModifiedProperties();
            
            EditorGUILayout.PropertyField(preserveAspectRatio);
            DrawRotateButton();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        protected virtual void DrawRotateButton()
        {
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();

            for (int i = 0; i < 4; i++)
            {
                var targetAngle = i * 90;
                var text = rotateId.intValue == i ? $"{targetAngle}° ❤️" : $"{targetAngle}°";
                if (GUILayout.Button(text, GUILayout.Height(30)) && rotateId.intValue != i)
                {
                    rotateId.intValue = i;
                    image.SetVerticesDirty();
                }
            }

            GUILayout.EndHorizontal();
        }
    }
}
#endif