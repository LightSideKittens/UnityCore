#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace LSCore
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class LSRaycastTarget : Graphic
    {
        protected override void UpdateGeometry() { }
        protected override void UpdateMaterial() { }
    }
    
#if UNITY_EDITOR
    
    [CustomEditor(typeof(LSRaycastTarget), true)]
    [CanEditMultipleObjects]
    public class LSRaycastTargetEditor : Editor
    {
        SerializedProperty raycast;
        SerializedProperty padding;
        
        protected virtual void OnEnable()
        {
            raycast = serializedObject.FindProperty("m_RaycastTarget");
            padding = serializedObject.FindProperty("m_RaycastPadding");
            SceneView.duringSceneGui += DrawAnchorsOnSceneView;
        }

        protected virtual void OnDisable()
        {
            SceneView.duringSceneGui -= DrawAnchorsOnSceneView;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(raycast);
            EditorGUILayout.PropertyField(padding);
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawAnchorsOnSceneView(SceneView sceneView) => DrawAnchorsOnSceneView(this, sceneView);

        public static void DrawAnchorsOnSceneView(Editor editor, SceneView sceneView)
        {
            if (!editor.target || editor.targets.Length > 1)
                return;

            if (!sceneView.drawGizmos)
                return;

            var graphic = editor.target as Graphic;
            RectTransform gui = graphic.rectTransform;
            Transform ownSpace = gui.transform;
            Rect rectInOwnSpace = gui.rect;

            Handles.color = Handles.UIColliderHandleColor;
            DrawRect(rectInOwnSpace, ownSpace, graphic.raycastPadding);
        }

        private static void DrawRect(Rect rect, Transform space, Vector4 offset)
        {
            Vector3 p0 = space.TransformPoint(new Vector2(rect.x + offset.x, rect.y + offset.y));
            Vector3 p1 = space.TransformPoint(new Vector2(rect.x + offset.x, rect.yMax - offset.w));
            Vector3 p2 = space.TransformPoint(new Vector2(rect.xMax - offset.z, rect.yMax - offset.w));
            Vector3 p3 = space.TransformPoint(new Vector2(rect.xMax - offset.z, rect.y + offset.y));

            Handles.DrawLine(p0, p1);
            Handles.DrawLine(p1, p2);
            Handles.DrawLine(p2, p3);
            Handles.DrawLine(p3, p0);
        }
    }
#endif
}