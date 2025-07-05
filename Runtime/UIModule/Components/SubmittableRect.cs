using System;
using UnityEngine;
using UnityEngine.EventSystems;
namespace LSCore
{
    public class SubmittableRect : MonoBehaviour, ISubmittable
    {
        [SerializeReference] public BaseSubmittableAnim anim;
        [SerializeReference] public BaseSubmittableDoIter doIter;
        [SerializeField] public ClickActions clickActions;

        public Transform Transform => transform;
        public event Action Submitted;
        [field: SerializeField] public ClickableStates States { get; private set; } = new();

        protected void Awake()
        {
            anim.Init(this);
            doIter.Init(this);
        }
        
        protected void OnDisable()
        {
            anim.OnDisable();
            doIter.OnDisable();
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            Submitted?.Invoke();
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
}