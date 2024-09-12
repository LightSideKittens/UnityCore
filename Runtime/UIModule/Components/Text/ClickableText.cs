using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
#endif


namespace LSCore
{
    public class ClickableText : LSText, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IClickable
    {
        [SerializeField] private ClickAnim anim;
        [SerializeField] private ClickActions clickActions;
        
        public ref ClickAnim Anim => ref anim;
        public Transform Transform => transform;
        public Action Clicked { get; set; }

        protected override void Awake()
        {
            base.Awake();
            anim.Init(transform);
        }

        protected override void Start()
        {
            base.Start();
            clickActions.Init();
        }
        
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            clickActions.OnClick();
            anim.OnClick();
            Clicked?.Invoke();
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData) => anim.OnPointerDown();
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData) => anim.OnPointerUp();

        protected override void OnDisable()
        {
            base.OnDisable();
            anim.OnDisable();
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(ClickableText), true), CanEditMultipleObjects]
    public class ClickableTextEditor : LSTextEditor
    {
        ClickableText text;
        private InspectorProperty clickActions;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            text = (ClickableText)target;
            clickActions = propertyTree.RootProperty.Children["clickActions"];
        }

        public override void OnInspectorGUI()
        {
            DrawTextPropertiesAsFoldout();
            propertyTree.BeginDraw(true);
            clickActions.Draw();
            propertyTree.EndDraw();
        }

        protected override void DrawExtraSettings()
        {
            base.DrawExtraSettings();
            text.Anim.Editor_Draw();
        }
    }
#endif
}