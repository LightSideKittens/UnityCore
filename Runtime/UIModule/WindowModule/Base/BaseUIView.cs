using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Scripting;

namespace LSCore
{
    public interface IUIView
    {
        RectTransform RectTransform { get; }
        WindowManager Manager { get; }
    }
    
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(RectTransform))]
    public abstract class BaseUIView<T> : SingleService<T>, IUIView where T : BaseUIView<T>
    {
        [SerializeReference] public ShowHideAnim showHideAnim = new DefaultUIViewAnimation();
        public RectTransform RectTransform { get; private set; }
        public virtual WindowManager Manager { get; } = new();
        
        [FoldoutGroup("Optional")]
        [SerializeReference] protected List<LSClickAction> clickActions;
        [field: SerializeField, FoldoutGroup("Optional")] protected virtual LSButton HomeButton { get; private set; }
        [field: SerializeField, FoldoutGroup("Optional")] protected virtual LSButton BackButton { get; private set; }
        [SerializeReference, FoldoutGroup("Optional/Events")] protected List<LSAction> onShowing;
        [SerializeReference, FoldoutGroup("Optional/Events")] protected List<LSAction> onHiding;
        [SerializeReference, FoldoutGroup("Optional/Events")] protected List<LSAction> onShowed;
        [SerializeReference, FoldoutGroup("Optional/Events")] protected List<LSAction> onHidden;
        

        protected virtual ShowWindowOption ShowOption { get; set; }
        protected virtual bool ActiveByDefault => false;

        [Preserve]
        protected void SetAnimation(ShowHideAnim anim) => showHideAnim = anim;
        [Preserve]
        protected void SetNeedDisableOnHidden(bool needDisableOnHidden) => Manager.needDisableOnHidden = needDisableOnHidden;

        protected override void Init()
        {
            base.Init();
            clickActions.Invoke();
            InitManager();
            showHideAnim?.Init();
            if (BackButton != null) BackButton.Clicked += OnBackButton;
            if (HomeButton != null) HomeButton.Clicked += OnHomeButton;

            var rectTransform = (RectTransform)transform;
            RectTransform = rectTransform;
        }

        protected virtual void InitManager()
        {
            Manager.Init(GetComponent<CanvasGroup>());
            Manager.Showing += OnShowing;
            Manager.Showed += OnShowed;
            Manager.Hiding += OnHiding;
            Manager.Hidden += OnHidden;
            Manager.showOption = () => ShowOption;
            Manager.showAnim = () => ShowAnim;
            Manager.hideAnim = () => HideAnim;
        }

        protected virtual void OnShowing() => onShowing.Invoke();
        protected virtual void OnHiding() => onHiding.Invoke();
        protected virtual void OnShowed() => onShowed.Invoke();
        protected virtual void OnHidden() => onHidden.Invoke();

        protected virtual Tween ShowAnim => showHideAnim?.Show;
        protected virtual Tween HideAnim => showHideAnim?.Hide;

        protected virtual void OnBackButton() => WindowsData.GoBack();

        protected virtual void OnHomeButton() => WindowsData.GoHome();

        public void AsHome() => WindowsData.SetHome(Manager);

        private static string logTag = $"[{typeof(T).Name}]".ToTag(new Color(0.38f, 1f, 0.33f));
        
        public void Show(ShowWindowOption option)
        {
            Burger.Log($"{logTag} {gameObject.name} Show with option {option}");
            ShowOption = option;
            Manager.Show();
        }

        public void Show()
        {
            Burger.Log($"{logTag} {gameObject.name} Show");
            Manager.Show();
        }
    }
}