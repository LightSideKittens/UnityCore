using System.Collections.Generic;
using DG.Tweening;
using LSCore.AnimationsModule;
using LSCore.AnimationsModule.Animations;
using UnityEngine;

namespace LSCore
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class BaseUIView<T> : SingleService<T>, ISerializationCallbackReceiver where T : BaseUIView<T>
    {
        public new AnimSequencer animation;
        public RectTransform RectTransform { get; private set; }
        protected virtual WindowManager Manager { get; } = new();

        [Header("Optional")] 
        [SerializeReference] protected List<LSClickAction> clickActions;
        [field: SerializeField] protected virtual LSButton HomeButton { get; private set; }
        [field: SerializeField] protected virtual LSButton BackButton { get; private set; }

        protected virtual ShowWindowOption ShowOption { get; set; }
        protected virtual bool UseDefaultAnimation => true;
        protected virtual bool ActiveByDefault => false;


        protected override void Init()
        {
            base.Init();
            clickActions.Invoke();
            InitManager();
            animation.InitAllAnims();
            if (BackButton != null) BackButton.Clicked += OnBackButton;
            if (HomeButton != null) HomeButton.Clicked += OnHomeButton;

            var rectTransform = (RectTransform)transform;
            RectTransform = rectTransform;
        }

        protected virtual void InitManager()
        {
            Manager.Init(gameObject);
            Manager.Showing += OnShowing;
            Manager.Showed += OnShowed;
            Manager.Hiding += OnHiding;
            Manager.Hidden += OnHidden;
            Manager.showOption = () => ShowOption;
            Manager.showAnim = () => ShowAnim;
            Manager.hideAnim = () => HideAnim;
        }

        protected virtual void OnShowing()
        {
        }

        protected virtual void OnHiding()
        {
        }

        protected virtual void OnShowed()
        {
        }

        protected virtual void OnHidden()
        {
        }

        protected virtual Tween ShowAnim => animation.Animate();

        protected virtual Tween HideAnim
        {
            get
            {
                var tween = animation.Animate();
                tween.Goto(tween.Duration(), true);
                tween.PlayBackwards();
                return tween;
            }
        }

        protected virtual void OnBackButton() => WindowsData.GoBack();

        protected virtual void OnHomeButton() => WindowsData.GoHome();

        public void AsHome() => WindowsData.SetHome(Manager);

        public void Show(ShowWindowOption option)
        {
            Burger.Log($"[{typeof(T)}] ({gameObject.name}) Show with option {option}");
            ShowOption = option;
            Show();
        }

        public void Show()
        {
            Burger.Log($"[{typeof(T)}] ({gameObject.name}) Show");
            Manager.Show();
        }

        public void OnBeforeSerialize()
        {
            if (this && animation != null && animation.Count == 0 && UseDefaultAnimation)
            {
                var anim = new AlphaAnim();
                var canvasGroupAnim = new CanvasGroupAlphaAnim();
                anim.canvasGroupAnim = canvasGroupAnim;
                canvasGroupAnim.target = GetComponent<CanvasGroup>();
                canvasGroupAnim.Duration = 0.2f;
                canvasGroupAnim.startValue = 0;
                canvasGroupAnim.endValue = 1;
                canvasGroupAnim.NeedInit = true;
                animation.Add(new AnimSequencer.AnimData()
                {
                    anim = anim,
                    time = 0,
                    timeOffset = 0,
                });
            }
        }

        public void OnAfterDeserialize()
        {

        }
    }
}