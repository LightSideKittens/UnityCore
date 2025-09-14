using System;
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
    public class LSToggle : LSImage, IToggle
    {
        [SerializeReference] public ShowHideAnim onOffAnim = new InOutShowHideAnim();
        [SerializeReference] private ToggleData isOn;
        
        [SerializeReference] public DefaultUIControl uiControl = new ();
        object IUIControlElement.UIControl => uiControl;
        
        public event Action Submitted
        {
            add => uiControl.Did += value;
            remove => uiControl.Did -= value;
        }
        
        public Action<bool> ValueChanged
        {
            get => isOn.valueChanged;
            set => isOn.valueChanged = value;
        }
        
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
            isOn.IsOn = value;
        }

        public void CallAndSub(Action<bool> action)
        {
            action(isOn);
            ValueChanged += action;
        }

        protected void OnValueChanged(bool value)
        {
#if UNITY_EDITOR
            if (World.IsEditMode) return;
#endif
            if (value)
            {
                onOffAnim.Show().SetId(this);
            }
            else
            {
                onOffAnim.Hide().SetId(this);
            }
        }
        
        protected override void Awake()
        {
            base.Awake();
#if UNITY_EDITOR
            if (World.IsEditMode) return;
#endif
            
            uiControl.Init(transform);
            uiControl.Did += () =>
            {
                isOn.IsOn = !isOn;
            };
        }

        protected override void OnEnable()
        {
            base.OnEnable();
#if UNITY_EDITOR
            if (World.IsEditMode) return;
#endif
            uiControl.OnEnable();
            CallAndSub(OnValueChanged);
            DOTweenExt.Complete(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
#if UNITY_EDITOR
            if (World.IsEditMode) return;
#endif
            ValueChanged -= OnValueChanged;
            uiControl.OnDisable();
        }
    }
    
#if UNITY_EDITOR

    [CustomEditor(typeof(LSToggle), true)]
    public class LSToggleEditor : LSImageEditor
    {
        private LSToggle toggle;
        private PropertyTree propertyTree;
        private InspectorProperty uiControl;
        private InspectorProperty isOn;
        private InspectorProperty onOffAnim;

        protected override void OnEnable()
        {
            base.OnEnable();
            toggle = (LSToggle)target;
            propertyTree = PropertyTree.Create(serializedObject);
            uiControl = propertyTree.RootProperty.Children["uiControl"];
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
            uiControl.Draw();
            propertyTree.EndDraw();
            serializedObject.ApplyModifiedProperties();
        }
    }
    
#endif
}