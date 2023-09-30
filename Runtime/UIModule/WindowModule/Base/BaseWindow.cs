using System;
using DG.Tweening;
using UnityEngine;

namespace LSCore
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(RectTransform))]
    public abstract class BaseWindow<T> : SingleService<T> where T : BaseWindow<T>
    {
        public static event Action Showing;
        public static event Action Hiding;

        public static event Action Showed;
        public static event Action Hidden;

        [SerializeField] protected float fadeSpeed = 0.2f;
        private CanvasGroup canvasGroup;
        private Tween showTween;
        private Tween hideTween;

        protected virtual Transform Parent => null;
        public RectTransform RectTransform { get; private set; }
        public static Canvas Canvas { get; private set; }
        public virtual int SortingOrder => 0;
        
        protected virtual float DefaultAlpha => 0;
        protected virtual bool ShowByDefault => false;

        protected override void Init()
        {
            base.Init();
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = DefaultAlpha;
            var canvas = GetComponent<Canvas>();
            Canvas = canvas;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.sortingOrder = SortingOrder + 30000;

            transform.SetParent(Parent, false);
            RectTransform = (RectTransform)transform;
            
            if(showTween.IsActive() || hideTween.IsActive()) return;
            if (ShowByDefault) Show();
            else if(!ShowByDefault) Hide();
        }

        private void InternalShow()
        {
            if (showTween.IsActive() && showTween.IsInitialized()) return;

            Showing?.Invoke();

            gameObject.SetActive(true);
            OnShowing();
            AnimateOnShowing(OnCompleteShow);
        }

        private void InternalHide()
        {
            if (hideTween.IsActive() && hideTween.IsInitialized()) return;
            Hiding?.Invoke();

            OnHiding();
            AnimateOnHiding(OnCompleteHide);
        }

        private void OnCompleteShow()
        {
            OnShowed();
            Showed?.Invoke();
        }

        private void OnCompleteHide()
        {
            gameObject.SetActive(false);
            OnHidden();
            Hidden?.Invoke();
        }

        protected virtual void OnShowing() { }
        protected virtual void OnHiding() { }
        protected virtual void OnShowed() {}
        protected virtual void OnHidden() { }

        private void AnimateOnShowing(TweenCallback onComplete)
        {
            hideTween?.Kill();
            showTween = ShowAnim.OnComplete(onComplete);
        }

        private void AnimateOnHiding(TweenCallback onComplete)
        {
            showTween?.Kill();
            hideTween = HideAnim.OnComplete(onComplete);
        }

        protected virtual Tween ShowAnim => canvasGroup.DOFade(1, fadeSpeed);

        protected virtual Tween HideAnim => canvasGroup.DOFade(0, fadeSpeed);

        public static void Show() => Instance.InternalShow();

        public static void Hide() => Instance.InternalHide();
    }
}
