using System;
using UnityEngine;
using UnityEngine.EventSystems;
namespace LSCore
{
    public class ClickableRect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IClickable
    {
        [SerializeField] private ClickAnim anim;
        [SerializeField] private ClickActions clickActions;
        
        public ref ClickAnim Anim => ref anim;
        public Transform Transform => transform;
        public Action Clicked { get; set; }

        protected void Awake()
        {
            anim.Init(transform);
        }

        protected void Start()
        {
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

        protected void OnDisable()
        {
            anim.OnDisable();
        }
    }
}