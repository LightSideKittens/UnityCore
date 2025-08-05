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
        [SerializeReference] private BaseToggleData isOn;
        
        [SerializeReference] public DefaultSubmittable submittable = new ();
        object ISubmittableElement.Submittable => submittable;
        
        public event Action Submitted
        {
            add => submittable.Submitted += value;
            remove => submittable.Submitted -= value;
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
            if (World.IsEditMode)
            {
                return;
            }
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
            if (!World.IsPlaying)
            {
                return;
            }
#endif
            
            submittable.Init(transform);
            submittable.Submitted += () =>
            {
                isOn.IsOn = !isOn;
            };
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            submittable.OnEnable();
            CallAndSub(OnValueChanged);
            DOTweenExt.Complete(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ValueChanged -= OnValueChanged;
            submittable.OnDisable();
        }
    }
    
#if UNITY_EDITOR

    [CustomEditor(typeof(LSToggle), true)]
    public class LSToggleEditor : LSImageEditor
    {
        private LSToggle toggle;
        private PropertyTree propertyTree;
        private InspectorProperty submittable;
        private InspectorProperty isOn;
        private InspectorProperty onOffAnim;

        protected override void OnEnable()
        {
            base.OnEnable();
            toggle = (LSToggle)target;
            propertyTree = PropertyTree.Create(serializedObject);
            submittable = propertyTree.RootProperty.Children["submittable"];
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
            submittable.Draw();
            propertyTree.EndDraw();
            serializedObject.ApplyModifiedProperties();
        }
    }
    
#endif
}