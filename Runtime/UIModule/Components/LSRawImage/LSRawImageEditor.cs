#if UNITY_EDITOR
using Sirenix.Utilities;
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
        SerializedProperty flip;

        protected override void OnEnable()
        {
            base.OnEnable();
            image = (LSRawImage)target;
            m_Texture = serializedObject.FindProperty("m_Texture");
            preserveAspectRatio = serializedObject.FindProperty("preserveAspectRatio");
            rotateId = serializedObject.FindProperty("rotateId");
            flip = serializedObject.FindProperty("flip");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Texture);
            var tex = image.texture;
            if (tex)
            {
                GUI.enabled = false;
                EditorGUILayout.Vector2IntField("Size", new Vector2Int(tex.width, tex.height));
                GUI.enabled = true;
            }
            AppearanceControlsGUI();
            RaycastControlsGUI();
            MaskableControlsGUI();
            SetShowNativeSize(m_Texture.objectReferenceValue != null, false);
            NativeSizeButtonGUI();

            serializedObject.ApplyModifiedProperties();
            
            EditorGUILayout.PropertyField(preserveAspectRatio);
            LSImageEditor.DrawFlipProperty(new GUIContent("Flip"), flip);
            LSImageEditor.DrawRotateButton(rotateId);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif