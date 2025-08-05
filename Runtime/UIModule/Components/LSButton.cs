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
    
    public class SubmittableStates
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
    public class DefaultSubmittable : ISubmittable
    {
        [SerializeReference] public BaseSubmittableAnim anim;
        [SerializeReference] public BaseSubmittableDoIter doIter = new DefaultSubmittableDoIter();
        [SerializeReference] public BaseSubmittableSelectBehaviour selectBehaviour;
        
        public Transform Transform { get; private set; }
        
        public event Action Submitted
        {
            add => doIter.onSubmitAction += value;
            remove => doIter.onSubmitAction -= value;
        }
        
        event Action ISubmittable.Submitted
        {
            add => submitted += value;
            remove => submitted -= value;
        }
        
        private Action submitted;
        public SubmittableStates States { get; private set; } = new();

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
            submitted?.Invoke();
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
            submitted?.Invoke();
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
    }
    
    public class LSButton : LSImage, ISubmittableElement
    {
        [SerializeReference] public DefaultSubmittable submittable = new ();
        object ISubmittableElement.Submittable => submittable;
        public event Action Submitted
        {
            add => submittable.Submitted += value;
            remove => submittable.Submitted -= value;
        }

        protected override void Awake()
        {
            base.Awake();
            submittable.Init(transform);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            submittable.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            submittable.OnDisable();
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(LSButton), true)]
    [CanEditMultipleObjects]
    public class LSButtonEditor : LSImageEditor
    {
        private LSButton button;
        private PropertyTree propertyTree;
        private InspectorProperty submittable;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            button = (LSButton)target;
            propertyTree = PropertyTree.Create(serializedObject);
            submittable = propertyTree.RootProperty.Children["submittable"];
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
            submittable.Draw();
            propertyTree.EndDraw();
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}