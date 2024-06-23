#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using LSCore.Extensions.Unity;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityToolbarExtender;
using UnityEngine.UI;

namespace LSCore
{
    [CustomEditor(typeof(LSImage), true)]
    [CanEditMultipleObjects]
    public class LSImageEditor : ImageEditor
    {
        SerializedProperty rotateId;

        SerializedProperty m_Sprite;
        SerializedProperty m_Type;
        SerializedProperty m_PreserveAspect;
        SerializedProperty m_UseSpriteMesh;
        SerializedProperty combineFilledWithSliced;
        SerializedProperty m_PixelsPerUnitMultiplier;
        FieldInfo bIsDriven;
        private LSImage image;
        private RectTransform rect;
        private bool isDragging;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_Sprite = serializedObject.FindProperty("m_Sprite");
            m_Type = serializedObject.FindProperty("m_Type");
            m_PreserveAspect = serializedObject.FindProperty("m_PreserveAspect");
            m_UseSpriteMesh = serializedObject.FindProperty("m_UseSpriteMesh");
            rotateId = serializedObject.FindProperty("rotateId");
            combineFilledWithSliced = serializedObject.FindProperty("combineFilledWithSliced");
            m_PixelsPerUnitMultiplier = serializedObject.FindProperty("m_PixelsPerUnitMultiplier");
            bIsDriven = typeof(ImageEditor).GetField("m_bIsDriven", BindingFlags.Instance | BindingFlags.NonPublic);
            image = (LSImage)target;
            rect = image.GetComponent<RectTransform>();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            bIsDriven.SetValue(this, (rect.drivenByObject as Slider)?.fillRect == rect);

            SpriteGUI();
            AppearanceControlsGUI();
            RaycastControlsGUI();
            MaskableControlsGUI();
            TypeGUI();

            if (image.type == Image.Type.Filled && image.fillMethod is Image.FillMethod.Horizontal or Image.FillMethod.Vertical)
            {
                EditorGUILayout.PropertyField(combineFilledWithSliced);
                EditorGUILayout.PropertyField(m_PixelsPerUnitMultiplier);
            }
            
            SetShowNativeSize(false);
            if (EditorGUILayout.BeginFadeGroup(m_ShowNativeSize.faded))
            {
                EditorGUI.indentLevel++;

                if ((Image.Type)m_Type.enumValueIndex == Image.Type.Simple)
                    EditorGUILayout.PropertyField(m_UseSpriteMesh);

                EditorGUILayout.PropertyField(m_PreserveAspect);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFadeGroup();
            NativeSizeButtonGUI();

            DrawRotateButton();

            serializedObject.ApplyModifiedProperties();
        }

        void SetShowNativeSize(bool instant)
        {
            Image.Type type = (Image.Type)m_Type.enumValueIndex;
            bool showNativeSize = (type == Image.Type.Simple || type == Image.Type.Filled) &&
                                  m_Sprite.objectReferenceValue != null;
            base.SetShowNativeSize(showNativeSize, instant);
        }

        protected virtual void DrawRotateButton()
        {
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();

            for (int i = 0; i < 4; i++)
            {
                var targetAngle = i * 90;
                if (GUILayout.Button($"{targetAngle}°", GUILayout.Height(30)) && rotateId.intValue != i)
                {
                    rotateId.intValue = i;
                    image.SetVerticesDirty();
                }
            }

            GUILayout.EndHorizontal();
        }

        [MenuItem("GameObject/LSCore/Image")]
        private static void CreateButton()
        {
            new GameObject("LSImage").AddComponent<LSImage>();
        }
    }

}
#endif