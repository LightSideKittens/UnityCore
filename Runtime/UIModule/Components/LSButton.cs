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
                    action?.Invoke(value);
                }
                this.value = value;
            }
        }
    }

    [Serializable]
    public class ClickableStates
    {
        private ReactBool select;
        private ReactBool press;
        private ReactBool hover;
        
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
    
    public class LSButton : LSImage, ISubmittable
    {
        [SerializeReference] public BaseSubmittableAnim anim;
        [SerializeReference] public BaseSubmittableDoIter doIter;
        [SerializeField] public ClickActions clickActions;

        public Transform Transform => transform;
        public event Action Submitted;
        [field: SerializeField] public ClickableStates States { get; private set; } = new();

        protected override void Awake()
        {
            base.Awake();
            anim.Init(this);
            doIter.Init(this);
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            anim.OnDisable();
            doIter.OnDisable();
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            Submitted?.Invoke();
            Debug.Log("OnPointerClick");
        }
        
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            States.Press = true;
            Debug.Log("OnPointerDown");
            EventSystem.current.SetSelectedGameObject(gameObject, eventData);
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            States.Press = false;
            Debug.Log("OnPointerUp");
        }
        
        void ISelectHandler.OnSelect(BaseEventData eventData)
        {
            States.Select = true;
            Debug.Log("OnSelect");
        }

        void IDeselectHandler.OnDeselect(BaseEventData eventData)
        {
            States.Select = false;
            Debug.Log("OnDeselect");
        }

        void ISubmitHandler.OnSubmit(BaseEventData eventData)
        {
            Submitted?.Invoke();
            Debug.Log("OnSubmit");
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            States.Hover = true;
            Debug.Log("OnPointerEnter");
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            States.Hover = false;
            Debug.Log("OnPointerExit");
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(LSButton), true)]
    [CanEditMultipleObjects]
    public class LSButtonEditor : LSImageEditor
    {
        private LSButton button;
        private PropertyTree propertyTree;
        private InspectorProperty clickActions;
        private InspectorProperty anim;
        private InspectorProperty doIter;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            button = (LSButton)target;
            propertyTree = PropertyTree.Create(serializedObject);
            anim = propertyTree.RootProperty.Children["anim"];
            doIter = propertyTree.RootProperty.Children["doIter"];
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
            anim.Draw();
            doIter.Draw();
            propertyTree.EndDraw();
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}