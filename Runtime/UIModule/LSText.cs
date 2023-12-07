using System;
using LightSideCore.Runtime.UIModule;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using System.Text.RegularExpressions;
using TMPro.EditorUtilities;
using UnityEditor;
#endif

namespace LSCore
{
    public class LSText : TextMeshProUGUI, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IClickable
    {
        [SerializeField] private ClickAnim anim;
        public ref ClickAnim Anim => ref anim;

        protected override void Awake()
        {
            base.Awake();
            anim.Init(transform);
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
        
        public Transform Transform => transform;
        public Action Clicked { get; set; }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(LSText), true), CanEditMultipleObjects]
    public class LSTextEditor : TMP_EditorPanelUI
    {
        SerializedProperty padding;
        LSText text;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            text = (LSText)target;
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
            text.Anim.Editor_Draw();
            if (Foldout.extraSettings)
            {
                EditorGUILayout.PropertyField(padding);
            }
        }
        
        [MenuItem("GameObject/LSCore/Text")]
        private static void CreateButton()
        {
            new GameObject("LSText").AddComponent<LSText>();
        }
    }
#endif
}