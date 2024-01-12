using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace LSCore
{
    public struct WindowsData
    {
        private static readonly Stack<Action> states = new();
        internal static Action hidePrevious;
        private static Action goHome;
        private static Action hideHome;
        private static bool recordStates;
        private static Action recordedState;
        internal static int sortingOrder;

        static WindowsData()
        {
            World.Destroyed += Clear;
        }
        
        internal static void GoBack() => states.Pop()();
        internal static void HidePrevious() => hidePrevious?.Invoke();

        internal static void StartRecording()
        {
            recordStates = true;
            recordedState = null;
        }

        internal static void Record(Action state)
        {
            if (recordStates)
            {
                recordedState += state;
            }
        }

        internal static void StopRecording()
        {
            recordStates = false;
            states.Push(recordedState);
        }

        internal static void GoHome()
        {
            states.Clear();
            var hideAction = hidePrevious;
            hidePrevious = null;
            sortingOrder = 0;
            goHome();
            var hideDelegate = (Delegate)hideHome;

            foreach (var delegat in hideAction.GetInvocationList())
            {
                if (delegat == hideDelegate)
                {
                    continue;
                }
                
                ((Action)delegat)();
            }

            hidePrevious = hideHome;
            sortingOrder = 1;
        }
        
        internal static void SetHome<T>(Action hide) where T : BaseWindow<T>
        {
            Clear();
            hideHome = hide;
            goHome += BaseWindow<T>.Show;
        }

        private static void Clear()
        {
            states.Clear();
            hidePrevious = null;
            goHome = null;
            hideHome = null;
            recordStates = false;
            recordedState = null;
            sortingOrder = 0;
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
        
        private static bool isCalledFromStatic;

        protected override void Init()
        {
            base.Init();
            
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = DefaultAlpha;
            
            var canvas = GetComponent<Canvas>();
            Canvas = canvas;
            
            var rectTransform = (RectTransform)transform;
            RectTransform = rectTransform;

            if (Parent != null && Parent.TryGetComponent<Canvas>(out _))
            {
                transform.SetParent(Parent, false);
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
            }

            canvas.overrideSorting = true;
            
            if (BackButton != null) BackButton.Clicked += OnBackButton;
            if (HomeButton != null) HomeButton.Clicked += OnHomeButton;
            
            if(isCalledFromStatic) return;
            if (ShowByDefault) Show();
            else InternalHide();
        }

        protected override void DeInit()
        {
            base.DeInit();
            isCalledFromStatic = false;
        }

        protected virtual void OnBackButton() => WindowsData.GoBack();

        protected virtual void OnHomeButton() => WindowsData.GoHome();

        private void InternalShow()
        {
            if (showTween != null) return;
            
            Showing?.Invoke();
            gameObject.SetActive(true);
            OnShowing();
            AnimateOnShowing(OnCompleteShow);
            RecordState();
            Canvas.sortingOrder = WindowsData.sortingOrder++;
        }

        protected virtual void RecordState()
        {
            if (NeedHidePrevious)
            {
                WindowsData.HidePrevious();
            }

            WindowsData.hidePrevious += InternalHide;
            WindowsData.Record(InternalHide);
        }

        private void InternalHide()
        {
            if (hideTween != null) return;

            WindowsData.sortingOrder--;
            WindowsData.hidePrevious -= InternalHide;
            WindowsData.Record(InternalShow);
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
            hideTween = null;
            showTween = ShowAnim.OnComplete(onComplete);
        }

        private void AnimateOnHiding(TweenCallback onComplete)
        {
            showTween?.Kill();
            showTween = null;
            hideTween = HideAnim.OnComplete(onComplete);
        }

        protected virtual Tween ShowAnim => canvasGroup.DOFade(1, fadeSpeed);

        protected virtual Tween HideAnim => canvasGroup.DOFade(0, fadeSpeed);

        public static void AsHome() => WindowsData.SetHome<T>(Instance.InternalHide);

        public static void Show()
        {
            WindowsData.StartRecording();
            isCalledFromStatic = true;
            Instance.InternalShow();
            WindowsData.StopRecording();
        }
    }
}
