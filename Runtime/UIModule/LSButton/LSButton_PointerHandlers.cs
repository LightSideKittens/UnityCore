using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace LSCore
{
    public partial class LSButton : IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        [SerializeField] private ClickAnim anim;
        public ref ClickAnim Anim => ref anim;

        protected override void Awake()
        {
            base.Awake();
            anim.Init(transform);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            anim.OnPointerClick();
            clicked?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData) => anim.OnPointerDown();
        public void OnPointerUp(PointerEventData eventData) => anim.OnPointerUp();

        protected override void OnDisable()
        {
            base.OnDisable();
            anim.OnDisable();
        }

        private Action clicked;
        public void Listen(Action action) => clicked += action;
        public void UnListen(Action action) => clicked -= action;
        public void UnListenAll(Action action) => clicked -= action;
    }
}