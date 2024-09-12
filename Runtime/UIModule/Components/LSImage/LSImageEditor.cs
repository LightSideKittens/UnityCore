#if UNITY_EDITOR
using System.Reflection;
using LSCore.Extensions;
using LSCore.Extensions.Unity;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
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
        FieldInfo bIsDriven;
        private LSImage image;
        private RectTransform rect;
        private bool isDragging;
        private bool showImageProperties;
        private static Texture2D texture;
        
        protected void DrawImagePropertiesAsFoldout()
        {
            texture ??= Texture2DExt.GetTextureByColor(new Color(0.2f, 0.19f, 0.29f));
            
            var old = SirenixGUIStyles.BoxContainer.normal.background;
            SirenixGUIStyles.BoxContainer.normal.background = texture;
            SirenixEditorGUI.BeginBox();
            SirenixEditorGUI.BeginBoxHeader();
            showImageProperties = SirenixEditorGUI.Foldout(showImageProperties, "Image Properties");
            SirenixEditorGUI.EndBoxHeader();
            if (SirenixEditorGUI.BeginFadeGroup(this, showImageProperties))
            {
                DrawImageProperties();
            }

            SirenixEditorGUI.EndFadeGroup();
            SirenixEditorGUI.EndBox();
            SirenixGUIStyles.BoxContainer.normal.background = old;
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
            
            EditorGUILayout.BeginHorizontal();
            
            Rect totalRect = EditorGUILayout.GetControlRect();
            Rect fieldRect = EditorGUI.PrefixLabel(totalRect, new GUIContent("Flip"));
            
            GUI.Label(fieldRect.TakeFromLeft(18), "X");
            var xFlipValue = EditorGUI.Toggle(fieldRect.TakeFromLeft(25), flip.vector2IntValue.x == 1);
            GUI.Label(fieldRect.TakeFromLeft(18), "Y");
            var yFlipValue = EditorGUI.Toggle(fieldRect.TakeFromLeft(25), flip.vector2IntValue.y == 1);
            flip.vector2IntValue = new Vector2Int(xFlipValue.ToInt(), yFlipValue.ToInt());

            EditorGUILayout.EndHorizontal();
            
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