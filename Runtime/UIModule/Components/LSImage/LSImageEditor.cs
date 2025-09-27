#if UNITY_EDITOR
using System;
using System.Reflection;
using LSCore.Extensions;
using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
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
        SerializedProperty flip;
        SerializedProperty mirror;
        SerializedProperty ignoreSLAAAAYout;
        FieldInfo bIsDriven;
        private LSImage image;
        private RectTransform rect;
        private bool isDragging;
        
        protected void DrawImagePropertiesAsFoldout()
        {
            EditorUtils.DrawInBoxFoldout("Image Properties", DrawImageProperties, image, false);
        }
        
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
            flip = serializedObject.FindProperty("flip");
            mirror = serializedObject.FindProperty("mirror");
            ignoreSLAAAAYout = serializedObject.FindProperty("ignoreSLAAAAYout");
            bIsDriven = typeof(ImageEditor).GetField("m_bIsDriven", BindingFlags.Instance | BindingFlags.NonPublic);
            image = (LSImage)target;
            rect = image.GetComponent<RectTransform>();
        }

        public override void OnInspectorGUI()
        {
            DrawImageProperties();
        }

        protected void DrawImageProperties()
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

            DrawFlipProperty(new GUIContent("Flip"), flip);
            DrawFlipProperty(new GUIContent("Mirror"), mirror);
            
            EditorGUILayout.PropertyField(ignoreSLAAAAYout, new GUIContent("Ignore SLAAAAYout"));
            
            DrawRotateButton(rotateId);
            
            serializedObject.ApplyModifiedProperties();
        }

        public static void DrawFlipProperty(GUIContent lbl, SerializedProperty prop)
        {
            Rect totalRect = EditorGUILayout.GetControlRect();
            EditorGUI.BeginProperty(totalRect, lbl, prop);
            prop.vector2IntValue = DrawFlipProperty(lbl, prop.vector2IntValue);
            EditorGUI.EndProperty();
        }

        public static void DrawRotateButton(SerializedProperty rotateId)
        {
            GUILayout.Space(10);
            var lbl = new GUIContent("Rotation");
            Rect totalRect = EditorGUILayout.GetControlRect();
            EditorGUI.BeginProperty(totalRect, lbl, rotateId);
            rotateId.intValue = DrawRotateButton(rotateId.intValue);
            EditorGUI.EndProperty();
        }

        public static Vector2Int DrawFlipProperty(GUIContent lbl, Vector2Int flip)
        {
            EditorGUILayout.BeginHorizontal();
            
            Rect totalRect = EditorGUILayout.GetControlRect();

            Rect fieldRect = EditorGUI.PrefixLabel(totalRect, lbl);
            
            GUI.Label(fieldRect.TakeFromLeft(18), "X");
            var xFlipValue = EditorGUI.Toggle(fieldRect.TakeFromLeft(25), flip.x == 1);
            GUI.Label(fieldRect.TakeFromLeft(18), "Y");
            var yFlipValue = EditorGUI.Toggle(fieldRect.TakeFromLeft(25), flip.y == 1);
            EditorGUILayout.EndHorizontal();
            return new Vector2Int(xFlipValue.ToInt(), yFlipValue.ToInt());
        }

        public static int DrawRotateButton(int id)
        {
            var totalRect = EditorGUILayout.GetControlRect(GUILayout.Height(30));
            GUILayout.BeginHorizontal();

            for (int i = 0; i < 4; i++)
            {
                var targetAngle = i * 90;
                var text = id == i ? $"{targetAngle}° ❤️" : $"{targetAngle}°";
                if (GUI.Button(totalRect.Split(i, 4), text) && id!= i)
                {
                    id = i;
                }
            }

            GUILayout.EndHorizontal();
            return id;
        }
        
        void SetShowNativeSize(bool instant)
        {
            Image.Type type = (Image.Type)m_Type.enumValueIndex;
            bool showNativeSize = (type == Image.Type.Simple || type == Image.Type.Filled) &&
                                  m_Sprite.objectReferenceValue != null;
            base.SetShowNativeSize(showNativeSize, instant);
        }

        [MenuItem("GameObject/LSCore/Image")]
        private static void CreateButton()
        {
            new GameObject("LSImage").AddComponent<LSImage>();
        }
    }

}
#endif