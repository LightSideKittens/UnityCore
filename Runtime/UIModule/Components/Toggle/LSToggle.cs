using System;
using System.Collections.Generic;
using DG.Tweening;
using LSCore.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif

namespace LSCore
{
    public class LSToggle : LSImage, IPointerClickHandler, IToggle
    {
        [SerializeReference] public ShowHideAnim onOffAnim = new InOutShowHideAnim();
        [SerializeField] private ClickActions clickActions;
        [SerializeReference] public List<LSAction> on = new();
        [SerializeReference] public List<LSAction> off = new();
        [SerializeField] private bool isOn;
        private bool lastIsOn;

        public Transform Transform => transform;
        public Action Clicked { get; set; }
        public Action<bool> ValueChanged { get; set; }
        
        /// <summary>
        /// Same as <code>Set(value);</code>
        /// </summary>
        /// <param name="value"></param>
        public bool IsOn
        {
            get => isOn;
            set => Set(value);
        }
        
        /// <summary>
        /// Same as <code>IsOn = value;</code>
        /// </summary>
        /// <param name="value"></param>
        public void Set(bool value)
        {
            if (value != isOn)
            {
                ForceSetState(value);
                Notify();
            }
        }
        
        public void SetSilently(bool value)
        {
            if (isOn != value)
            {
                ForceSetState(value);
            }
        }

        public void CallAndSub(Action<bool> action)
        {
            action(isOn);
            ValueChanged += action;
        }

        protected void ForceSetState(bool value)
        {
            lastIsOn = isOn;
            isOn = value;
            OnValueChanged();
        }
        
        protected void Notify()
        {
            ValueChanged?.Invoke(isOn);
        }

        protected void OnValueChanged()
        {
#if UNITY_EDITOR
            if (World.IsEditMode)
            {
                return;
            }
#endif
            if (isOn)
            {
                onOffAnim.Show().SetId(this);
                on.Invoke();
            }
            else
            {
                onOffAnim.Hide().SetId(this);
                off.Invoke();
            }
        }
        
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if(!eventData.IsFirstTouch()) return;
            clickActions.OnClick();
            ForceSetState(!isOn);
            Notify();
            Clicked?.Invoke();
        }

        protected override void Awake()
        {
            base.Awake();
#if UNITY_EDITOR
            if (!World.IsPlaying)
            {
                return;
            }
#endif
            
            onOffAnim.Init();
            ForceSetState(isOn);
            DOTweenExt.Complete(this);
        }

        protected override void Start()
        {
            base.Start();
            clickActions.Init();
        }

        protected override void OnDidApplyAnimationProperties()
        {
            base.OnDidApplyAnimationProperties();
            if (lastIsOn != isOn)
            {
                ForceSetState(isOn);
                Notify();
            }
        }
    }
    
#if UNITY_EDITOR

    [CustomEditor(typeof(LSToggle), true)]
    public class LSToggleEditor : LSImageEditor
    {
        private LSToggle toggle;
        private PropertyTree propertyTree;
        private InspectorProperty clickedActions;
        private InspectorProperty on;
        private InspectorProperty off;
        private InspectorProperty isOn;
        private InspectorProperty onOffAnim;

        protected override void OnEnable()
        {
            base.OnEnable();
            toggle = (LSToggle)target;
            propertyTree = PropertyTree.Create(serializedObject);
            clickedActions = propertyTree.RootProperty.Children["clickedActions"];
            on = propertyTree.RootProperty.Children["on"];
            off = propertyTree.RootProperty.Children["off"];
            isOn = propertyTree.RootProperty.Children["isOn"];
            onOffAnim = propertyTree.RootProperty.Children["onOffAnim"];
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
            onOffAnim.Draw();
            isOn.Draw();
            on.Draw();
            off.Draw();
            clickedActions.Draw();
            propertyTree.EndDraw();
            serializedObject.ApplyModifiedProperties();
        }
    }
    
#endif
}