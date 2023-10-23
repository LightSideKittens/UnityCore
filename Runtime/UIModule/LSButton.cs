using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LSCore
{
    public class LSButton : Button
    {
        private ClickAnim anim;

        protected override void Awake()
        {
            base.Awake();
            anim = new ClickAnim(transform);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            anim.OnPointerDown();
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            anim.OnPointerClick();
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            anim.OnPointerUp();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            anim.OnDisable();
        }
    }
}