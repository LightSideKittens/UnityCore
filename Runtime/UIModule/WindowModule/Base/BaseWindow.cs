using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace LSCore
{
    public struct WindowsData
    {
        internal static readonly Stack<(Action show, bool needHide)> showPrevious = new();
        internal static Action hidePrevious;
        private static Action goHome;

        internal static void ShowPrevious()
        {
            if(showPrevious.Count == 0) return;
            
            var last = showPrevious.Pop();
            List<Action> result = new() {last.show};
            
            for (int i = 0; i < showPrevious.Count; i++)
            {
                if (last.needHide) break;
                last = showPrevious.Pop();
                result.Add(last.show);
            }

            for (int i = result.Count - 1; i >= 0; i--)
            {
                result[i]();
            }
        }

        internal static void GoHome()
        {
            goHome();
            showPrevious.Clear();
        }
        
        static WindowsData()
        {
            World.Destroyed += () =>
            {
                showPrevious.Clear();
                hidePrevious = null;
                goHome = null;
            };
        }
        
        public static void SetHome<T>() where T : BaseWindow<T>
        {
            goHome = BaseWindow<T>.ExcludeFromHidePrevious;
            goHome += BaseWindow<T>.Show;
        }
    }
    
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

        protected virtual Transform Parent => IsExistsInManager ? DaddyCanvas.Instance.transform : null;
        [field: Header("Optional")]
        [field: SerializeField] protected virtual LSButton HomeButton { get; private set; }
        [field: SerializeField] protected virtual LSButton BackButton { get; private set; }
        public RectTransform RectTransform { get; private set; }
        public static Canvas Canvas { get; private set; }
        public virtual int SortingOrder => 0;
        
        protected virtual float DefaultAlpha => 0;
        protected virtual bool ShowByDefault => false;
        protected virtual bool NeedHidePrevious => true;
        protected virtual bool NeedShowPrevious => true;
        
        private static bool isCalledFromStatic;

        protected override void Init()
        {
            base.Init();
            
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = DefaultAlpha;
            
            transform.SetParent(Parent, false);
            
            var canvas = GetComponent<Canvas>();
            Canvas = canvas;

            var rectTransform = (RectTransform)transform;
            RectTransform = rectTransform;

            if (Parent != null && Parent.TryGetComponent<Canvas>(out _))
            {
                var zero = Vector2.zero;
                var one = Vector2.one;
                
                rectTransform.anchorMin = zero;
                rectTransform.anchorMax = one;
                rectTransform.anchoredPosition = zero;
                rectTransform.offsetMin = zero;
                rectTransform.offsetMax = zero;
                rectTransform.localScale = one;
            }
            else
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = Camera.main;
                canvas.sortingOrder = SortingOrder + 30000;
            }

            if (BackButton != null) BackButton.Clicked += OnBackButton;
            if (HomeButton != null) HomeButton.Clicked += OnHomeButton;
            
            if(isCalledFromStatic) return;
            if (ShowByDefault) Show();
            else Hide();
        }

        protected override void DeInit()
        {
            base.DeInit();
            isCalledFromStatic = false;
        }

        protected virtual void OnBackButton()
        {
            WindowsData.hidePrevious -= Hide;
            if (NeedShowPrevious) WindowsData.ShowPrevious();
            if (hideTween.IsActive()) return;
            StartHiding();
        }

        protected virtual void OnHomeButton() => WindowsData.GoHome();

        private void InternalShow()
        {
            if (showTween.IsActive()) return;
            
            Action hide = WindowsData.hidePrevious;
            WindowsData.hidePrevious += Hide;
            
            Showing?.Invoke();
            gameObject.SetActive(true);
            OnShowing();
            AnimateOnShowing(OnCompleteShow);

            if (NeedHidePrevious) hide?.Invoke();
        }

        private void InternalHide()
        {
            if (hideTween.IsActive()) return;
            
            WindowsData.hidePrevious -= Hide;
            WindowsData.showPrevious.Push((Show, NeedHidePrevious));

            StartHiding();
        }

        private void StartHiding()
        {
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

        public static void Show()
        {
            isCalledFromStatic = true;
            Instance.InternalShow();
        }

        private static void Hide()
        {
            isCalledFromStatic = true;
            Instance.InternalHide();
        }

        internal static void ExcludeFromHidePrevious() => WindowsData.hidePrevious -= Hide;
    }
}
