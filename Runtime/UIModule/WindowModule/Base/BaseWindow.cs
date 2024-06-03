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

        protected virtual Transform Parent => IsExistsInManager ? DaddyCanvas.Instance.transform : null;
        [field: Header("Optional")]
        [field: SerializeField] protected virtual LSButton HomeButton { get; private set; }
        [field: SerializeField] protected virtual LSButton BackButton { get; private set; }
        [field: SerializeField] protected virtual bool NeedHideAllPrevious { get; private set; }
        public RectTransform RectTransform { get; private set; }
        public static Canvas Canvas { get; private set; }

        protected virtual float DefaultAlpha => 0;
        protected virtual bool ActiveByDefault => false;

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
                
                Rect safeArea = Screen.safeArea;

                Vector2 anchorMin = safeArea.position;
                Vector2 anchorMax = safeArea.position + safeArea.size;
                anchorMin.x /= Screen.width;
                anchorMin.y /= Screen.height;
                anchorMax.x /= Screen.width;
                anchorMax.y /= Screen.height;

                rectTransform.anchorMin = anchorMin;
                rectTransform.anchorMax = anchorMax;
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
            gameObject.SetActive(ActiveByDefault);
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

            AnimateOnShowing(OnCompleteShow);
            Showing?.Invoke();
            gameObject.SetActive(true);
            OnShowing();
            RecordState();
        }

        protected virtual void RecordState()
        {
            if (NeedHideAllPrevious)
            {
                WindowsData.HideAllPrevious();
            }

            WindowsData.hideAllPrevious += InternalHide;
            WindowsData.Record(InternalHide);
            Canvas.sortingOrder = WindowsData.sortingOrder++;
        }

        private void InternalHide()
        {
            if (hideTween != null) return;

            AnimateOnHiding(OnCompleteHide);
            WindowsData.sortingOrder--;
            WindowsData.hideAllPrevious -= InternalHide;
            WindowsData.Record(InternalShow);
            Hiding?.Invoke();
            OnHiding();
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

        public static void AsHome() => WindowsData.SetHome<T>();

        public static void Show()
        {
            isCalledFromStatic = true;
            if (WindowsData.IsPreLast(Instance) && WindowsData.sortingOrder - 1 > Canvas.sortingOrder)
            {
                WindowsData.GoBack();
                return;
            }
            WindowsData.StartRecording();
            Instance.InternalShow();
            WindowsData.StopRecording();
        }

        internal static void GoHome()
        {
            WindowsData.StartRecording();
            WindowsData.HideAllPrevious();
            Show();
            WindowsData.StopRecording();
        }
    }
}
