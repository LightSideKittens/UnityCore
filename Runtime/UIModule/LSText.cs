using System;
using TMPro;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using TMPro.EditorUtilities;
using UnityEditor;
#endif

namespace LSCore
{
    public class LSText : TextMeshProUGUI, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        private ClickAnim anim;
        public event Action Clicked;

        protected override void Awake()
        {
            base.Awake();
            anim = new ClickAnim(transform);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            anim.OnPointerClick();
            Clicked?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData) => anim.OnPointerDown();
        public void OnPointerUp(PointerEventData eventData) => anim.OnPointerUp();

        protected override void OnDisable()
        {
            base.OnDisable();
            anim.OnDisable();
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(LSText), true), CanEditMultipleObjects]
    public class LSTextEditor : TMP_EditorPanelUI
    {
        SerializedProperty padding;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            padding = serializedObject.FindProperty("m_RaycastPadding");
            SceneView.duringSceneGui += DrawAnchorsOnSceneView;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            SceneView.duringSceneGui -= DrawAnchorsOnSceneView;
        }

        private void DrawAnchorsOnSceneView(SceneView sceneView) => LSRaycastTargetEditor.DrawAnchorsOnSceneView(this, sceneView);

        
        protected override void DrawExtraSettings()
        {
            base.DrawExtraSettings();
            if (Foldout.extraSettings)
            {
                EditorGUILayout.PropertyField(padding);
            }
        }
    }
#endif
}