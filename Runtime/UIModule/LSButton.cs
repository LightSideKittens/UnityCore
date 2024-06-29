using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;

namespace LSCore
{
    public class LSButton : LSImage,  IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IClickable
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
            Clicked?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData) => anim.OnPointerDown();
        public void OnPointerUp(PointerEventData eventData) => anim.OnPointerUp();

        protected override void OnDisable()
        {
            base.OnDisable();
            anim.OnDisable();
        }

        public Transform Transform => transform;
        public Action Clicked { get; set; }
    }
    
#if UNITY_EDITOR
    
    [CustomEditor(typeof(LSButton), true)]
    [CanEditMultipleObjects]
    public class LSButtonEditor : LSImageEditor
    {
        private LSButton button;
        protected override void OnEnable()
        {
            base.OnEnable();
            button = (LSButton)target;
        }

        protected override void DrawRotateButton()
        {
            button.Anim.Editor_Draw();
            base.DrawRotateButton();
        }

        [MenuItem("GameObject/LSCore/Button")]
        private static void CreateButton()
        {
            new GameObject("LSButton").AddComponent<LSButton>();
        }
    }
#endif
}