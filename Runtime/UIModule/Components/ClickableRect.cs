using System;
using UnityEngine;
using UnityEngine.EventSystems;
namespace LSCore
{
    public class ClickableRect : MonoBehaviour, IClickable
    {
        [SerializeField] private BaseClickAnim anim;
        [SerializeField] private ClickActions clickActions;
        
        public Transform Transform => transform;
        public Action Submitted { get; set; }
        public Action<bool> SelectChanged { get; set; }
        public Action<bool> HoverChanged { get; set; }
        public Action<bool> ActiveChanged { get; set; }

        protected void Start()
        {
            clickActions.Init();
        }
        
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (!eventData.IsFirstTouch()) return;
            clickActions.OnClick();
            anim.OnClick();
            Submitted?.Invoke();
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (!eventData.IsFirstTouch()) return;
            anim.OnDown();
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (!eventData.IsFirstTouch()) return;
            anim.OnUp();
        }

        protected void OnDisable()
        {
            anim.OnDisable();
        }

        void ISelectHandler.OnSelect(BaseEventData eventData)
        {
            
        }

        void IDeselectHandler.OnDeselect(BaseEventData eventData)
        {
            
        }

        void ISubmitHandler.OnSubmit(BaseEventData eventData)
        {
            
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            
        }
    }
}