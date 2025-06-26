﻿using System;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;

namespace LSCore
{
    public class LSButton : LSImage,  IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IClickable
    {
        [SerializeField] private ClickAnim anim;
        [SerializeField] public ClickActions clickActions;
        
        public ref ClickAnim Anim => ref anim;

        protected override void Awake()
        {
            base.Awake();
            anim.Init(transform);
        }

        protected override void Start()
        {
            base.Start();
            clickActions?.Init();
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (!eventData.IsFirstTouch()) return;
            clickActions?.OnClick();
            anim.OnClick();
            Clicked?.Invoke();
        }
        
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (!eventData.IsFirstTouch()) return;
            anim.OnPointerDown();
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (!eventData.IsFirstTouch()) return;
            anim.OnPointerUp();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            anim.OnDisable();
        }

        public Transform Transform => transform;
        public Action Clicked { get; set; }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(LSButton), true)]
    [CanEditMultipleObjects]
    public class LSButtonEditor : LSImageEditor
    {
        private LSButton button;
        private PropertyTree propertyTree;
        private InspectorProperty clickActions;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            button = (LSButton)target;
            propertyTree = PropertyTree.Create(serializedObject);
            clickActions = propertyTree.RootProperty.Children["clickActions"];
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            propertyTree?.Dispose();
        }

        public override void OnInspectorGUI()
        {
            DrawImagePropertiesAsFoldout();
            propertyTree.BeginDraw(true);
            clickActions.Draw();
            propertyTree.EndDraw();
            serializedObject.ApplyModifiedProperties();
        }

        protected override void DrawRotateButton()
        {
            button.Anim.Editor_Draw(target);
            base.DrawRotateButton();
        }
    }
#endif
}