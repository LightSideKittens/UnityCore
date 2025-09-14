using System;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;

namespace LSCore
{
    public struct ReactBool
    {
        private bool value;
        public Action<bool> action;

        public static implicit operator bool(ReactBool reactBool) => reactBool.value;
        
        public static ReactBool operator +(ReactBool a, Action<bool> b)
        {
            a.action += b;
            return a;
        }
        
        public static ReactBool operator -(ReactBool a, Action<bool> b)
        {
            a.action -= b;
            return a;
        }
        
        public bool Value
        {
            get => value;
            set
            {
                if (this.value != value)
                { 
                    this.value = value;
                    action?.Invoke(value);
                }
            }
        }
    }
    
    public class UIControlStates
    {
        private ReactBool select;
        private ReactBool press;
        private ReactBool hover;
        public BaseEventData currentEventData;
        
        public event Action<bool> SelectChanged
        {
            add => select += value;
            remove => select -= value;
        }
        
        public event Action<bool> PressChanged
        {
            add => press += value;
            remove => press -= value;
        }
        
        public event Action<bool> HoverChanged
        {
            add => hover += value;
            remove => hover -= value;
        }
        
        public bool Select
        {
            get => select;
            set => select.Value = value;
        }
        
        public bool Press
        {
            get => press;
            set => press.Value = value;
        }

        public bool Hover
        {
            get => hover;
            set => hover.Value = value;
        }
    }

    [Serializable]
    public class DefaultUIControl : IUIControl
    {
        [SerializeReference] public BaseUIControlAnim anim;
        [SerializeReference] public BaseUIControlDoIter doIter = new DefaultUIControlDoIter();
        [SerializeReference] public BaseUIControlSelectBehaviour selectBehaviour;
        
        public Transform Transform { get; private set; }
        
        public event Action Did
        {
            add => doIter.onActivateAction += value;
            remove => doIter.onActivateAction -= value;
        }
        
        public event Action Activated
        {
            add => activated += value;
            remove => activated -= value;
        }
        
        private Action activated;
        public UIControlStates States { get; private set; } = new();

        public void Init(Transform transform)
        {
            Transform = transform;
            anim?.Init(this);
            doIter?.Init(this);
            selectBehaviour?.Init(this);
        }

        public void OnEnable()
        {
            anim?.OnEnable();
            doIter?.OnEnable();
            selectBehaviour?.OnEnable();
        }

        public void OnDisable()
        {
            anim?.OnDisable();
            doIter?.OnDisable();
            selectBehaviour?.OnDisable();
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            States.currentEventData = eventData;
            Activate();
        }
        
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            States.currentEventData = eventData;
            States.Press = true;
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            States.currentEventData = eventData;
            States.Press = false;
        }
        
        void ISelectHandler.OnSelect(BaseEventData eventData)
        {
            States.currentEventData = eventData;
            States.Select = true;
        }

        void IDeselectHandler.OnDeselect(BaseEventData eventData)
        {
            States.currentEventData = eventData;
            States.Select = false;
        }
        
        void ISubmitHandler.OnSubmit(BaseEventData eventData)
        {
            States.currentEventData = eventData;
            Activate();
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            States.currentEventData = eventData;
            States.Hover = true;
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            States.currentEventData = eventData;
            States.Hover = false;
        }

        void IMoveHandler.OnMove(AxisEventData eventData)
        {
            States.currentEventData = eventData;
            selectBehaviour.OnMove(eventData);
        }

        public void Activate()
        {
            activated?.Invoke();
        }
    }
    
    public class LSButton : LSImage, IUIControlElement
    {
        [SerializeReference] public DefaultUIControl uiControl = new ();
        object IUIControlElement.UIControl => uiControl;
        
        public event Action Did
        {
            add => uiControl.Did += value;
            remove => uiControl.Did -= value;
        }
        
        public void Do() => uiControl.doIter.Do();
        public void Activate() => uiControl.Activate();

        protected override void Awake()
        {
            base.Awake();
            uiControl.Init(transform);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            uiControl.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            uiControl.OnDisable();
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(LSButton), true)]
    [CanEditMultipleObjects]
    public class LSButtonEditor : LSImageEditor
    {
        private LSButton button;
        private PropertyTree propertyTree;
        private InspectorProperty uiControl;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            button = (LSButton)target;
            propertyTree = PropertyTree.Create(serializedObject);
            uiControl = propertyTree.RootProperty.Children["uiControl"];
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
            uiControl.Draw();
            propertyTree.EndDraw();
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}