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
        SerializedProperty invert;
        SerializedProperty gradient;
        SerializedProperty angle;
        SerializedProperty gradientStart;
        SerializedProperty gradientEnd;

        SerializedProperty m_Sprite;
        SerializedProperty m_Type;
        SerializedProperty m_PreserveAspect;
        SerializedProperty m_UseSpriteMesh;
        FieldInfo m_bIsDriven;
        private LSImage image;
        private RectTransform rect;
        private bool isDragging;
        private bool isEditing;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_Sprite = serializedObject.FindProperty("m_Sprite");
            m_Type = serializedObject.FindProperty("m_Type");
            m_PreserveAspect = serializedObject.FindProperty("m_PreserveAspect");
            m_UseSpriteMesh = serializedObject.FindProperty("m_UseSpriteMesh");
            rotateId = serializedObject.FindProperty("rotateId");
            invert = serializedObject.FindProperty("invert");
            gradient = serializedObject.FindProperty("gradient");
            angle = serializedObject.FindProperty("angle");
            gradientStart = serializedObject.FindProperty("gradientStart");
            gradientEnd = serializedObject.FindProperty("gradientEnd");
            var type = GetType().BaseType;
            m_bIsDriven = type.GetField("m_bIsDriven", BindingFlags.Instance | BindingFlags.NonPublic);
            image = (LSImage)target;
            rect = image.GetComponent<RectTransform>();
            isEditing = false;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            //m_bIsDriven.SetValue(this, (rect.drivenByObject as Slider)?.fillRect == rect);

            SpriteGUI();
            EditorGUILayout.PropertyField(gradient);
            EditorGUILayout.PropertyField(invert);
            if (GUILayout.Button(isEditing ? "Stop Edit" : "Edit"))
            {
                isEditing = !isEditing;
            }

            EditorGUILayout.PropertyField(m_Material);
            RaycastControlsGUI();
            MaskableControlsGUI();

            TypeGUI();
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
                if (GUILayout.Button($"{targetAngle}°") && rotateId.intValue != i)
                {
                    rotateId.intValue = i;
                    image.SetColorDirty();
                }
            }

            GUILayout.EndHorizontal();
        }

        private Dictionary<int, int> map = new();

        void OnSceneGUI()
        {
            if (!isEditing) return;

            var imageTransform = image.transform;
            var imagePosition = imageTransform.position;
            var imageRotation = imageTransform.rotation.eulerAngles.z;

            Vector3 center = (Vector3)image.rectTransform.rect.center + imagePosition;
            var center2D = (Vector2)center;

            var start = (image.GradientStartPoint + center2D).RotateAroundPoint(imagePosition, imageRotation);
            var end = (image.GradientEndPoint + center2D).RotateAroundPoint(imagePosition, imageRotation);
            center = center2D.RotateAroundPoint(imagePosition, imageRotation);

            float handle1Size = HandleUtility.GetHandleSize(start) * 0.1f;
            float handle2Size = HandleUtility.GetHandleSize(end) * 0.1f;
            Handles.DrawSolidDisc(start, Vector3.forward, handle1Size);
            Handles.DrawSolidDisc(end, Vector3.forward, handle2Size);
            var id1 = GUIUtility.GetControlID(FocusType.Passive);
            var id2 = GUIUtility.GetControlID(FocusType.Passive);
            if (id1 == -1) return;
            map.Clear();
            map.Add(id1, 0);
            map.Add(id2, 1);
            HandleInput(id1);
            HandleInput(id2);
            EditorGUI.BeginChangeCheck();
            Vector3 newHandle1Pos = Handles.FreeMoveHandle(id1, start, handle1Size * 1.5f, Vector3.zero,
                Handles.CircleHandleCap);
            Vector3 newHandle2Pos =
                Handles.FreeMoveHandle(id2, end, handle2Size * 1.5f, Vector3.zero, Handles.CircleHandleCap);

            if (EditorGUI.EndChangeCheck())
            {
                newHandle1Pos = ((Vector2)newHandle1Pos).RotateAroundPoint(imagePosition, -imageRotation);
                newHandle2Pos = ((Vector2)newHandle2Pos).RotateAroundPoint(imagePosition, -imageRotation);
                newHandle1Pos -= center;
                newHandle2Pos -= center;
                image.GradientStart = newHandle1Pos.UnclampedInverseLerp(image.minPoint, image.maxPoint);
                image.GradientEnd = newHandle2Pos.UnclampedInverseLerp(image.maxPoint, image.minPoint);
                Undo.RecordObject(image, "Move Handle");
                EditorUtility.SetDirty(image);
            }

            EditorGUI.BeginChangeCheck();
            var newAngle = Handles.RotationHandle(Quaternion.Euler(Vector3.forward * (image.Angle + imageRotation)),
                center);
            if (EditorGUI.EndChangeCheck())
            {
                image.Angle = newAngle.eulerAngles.z - imageRotation;
                EditorUtility.SetDirty(image);
            }
        }

        private static readonly float DoubleClickTime = 0.3f; // Time threshold for double click
        private float lastClickTime = 0f;
        private int currentControlId;

        private void HandleInput(int controlId)
        {
            Event e = Event.current;
            var type = e.GetTypeForControl(controlId);

            switch (type)
            {
                case EventType.MouseDown:
                    if (HandleUtility.nearestControl == controlId && e.button == 0) // Left mouse button
                    {
                        float timeSinceLastClick = (float)(EditorApplication.timeSinceStartup - lastClickTime);

                        if (timeSinceLastClick < DoubleClickTime)
                        {
                            OpenColorPicker(map[controlId]);
                        }

                        lastClickTime = (float)EditorApplication.timeSinceStartup;
                    }

                    break;
                case EventType.ExecuteCommand:
                    if (e.commandName == "ColorPickerChanged")
                    {
                        var colors = image.Gradient.colorKeys;
                        var alphaKeys = image.Gradient.alphaKeys;
                        colors[currentControlId].color = LSColorPicker.Color;
                        alphaKeys[currentControlId].alpha = LSColorPicker.Color.a;
                        image.Gradient.SetKeys(colors, alphaKeys);
                        image.SetVerticesDirty();
                    }

                    break;
            }
        }

        private void OpenColorPicker(int cotrolID)
        {
            currentControlId = cotrolID;
            var color = image.Gradient.colorKeys[cotrolID].color;
            color.a = image.Gradient.alphaKeys[cotrolID].alpha;

            LSColorPicker.Show(newColor => { }, color);
        }

        [MenuItem("GameObject/LSCore/Image")]
        private static void CreateButton()
        {
            new GameObject("LSImage").AddComponent<LSImage>();
        }
    }

}
#endif