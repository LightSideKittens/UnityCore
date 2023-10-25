using System;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.UI;
#endif
using UnityEngine;
using UnityEngine.EventSystems;

namespace LSCore
{
    public class LSButton : LSImage,  IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
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
        public void OnClick(Action action) => clicked = action;
        public void Listen(Action action) => clicked += action;
        public void UnListen(Action action) => clicked -= action;
        public void UnListenAll() => clicked = null;
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