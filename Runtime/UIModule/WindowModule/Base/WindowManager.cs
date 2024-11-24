using System;
using DG.Tweening;
using LSCore.Extensions.Unity;
using UnityEngine;

namespace LSCore
{
    public class WindowManager
    {
        public event Action Showing;
        public event Action Hiding;

        public event Action Showed;
        public event Action Hidden;
        
        public static void FitInSafeArea(RectTransform target)
        {
            var parent = (RectTransform)target.root;
            var zero = Vector2.zero;
            var one = Vector2.one;

            Rect safeArea = Screen.safeArea;
            float xFactor = parent.rect.width / LSScreen.Width;
            float yFactor = parent.rect.height / LSScreen.Height;

            target.anchorMin = zero;
            target.anchorMax = one;
            target.anchoredPosition = zero;
            target.localScale = Vector3.one;

            target.offsetMin = safeArea.min * xFactor;
            target.offsetMax = (safeArea.max - new Vector2(LSScreen.Width, LSScreen.Height)) * yFactor;
        }

        private Tween showTween;
        private Tween hideTween;
        private GameObject gameObject;

        public Func<ShowWindowOption> showOption;
        public Func<Tween> showAnim;
        public Func<Tween> hideAnim;
        public Canvas canvas;
        public bool needDisableOnHidden = true;
        private CanvasGroup canvasGroup;
        public bool IsShow { get; private set; }

        public void Init(CanvasGroup canvasGroup)
        {
            this.gameObject = canvasGroup.gameObject;
            this.canvasGroup = canvasGroup;
        }
        
        public void Show()
        {
            if (WindowsData.IsAt(this, 0))
            {
                Burger.Log($"{gameObject.name} Show WindowsData.IsAt");
                WindowsData.GoBack();
                return;
            }
            WindowsData.StartRecording();
            InternalShow();
            WindowsData.StopRecording();
        }

        internal void HideAllPreviousAndShow()
        {
            WindowsData.HideAllPrevious();
            Show();
        }
        
        private void InternalShow()
        {
            if (showTween != null) return;
            Burger.Log($"{gameObject.name} InternalShow");
            IsShow = true;
            WindowsData.CallOption(showOption());
            RecordState();
            if (canvas)
            {
                canvas.sortingOrder = WindowsData.sortingOrder++;
            }
            
            Showing?.Invoke();
            AnimateOnShowing(OnCompleteShow);
            gameObject.SetActive(true);
        }

        protected virtual void RecordState()
        {
            WindowsData.hidePrevious = InternalHide;
            WindowsData.hideAllPrevious += InternalHide;
            WindowsData.Record(InternalHide);
        }

        private void InternalHide()
        {
            if (hideTween != null) return;
            Burger.Log($"{gameObject.name} InternalHide");
            IsShow = false;
            if (canvas)
            {
                WindowsData.sortingOrder--;
            }
            WindowsData.hideAllPrevious -= InternalHide;
            WindowsData.Record(InternalShow);
            
            Hiding?.Invoke();
            AnimateOnHiding(OnCompleteHide);
        }
        
        private void OnCompleteShow()
        {
            Showed?.Invoke();
        }

        private void OnCompleteHide()
        {
            if (needDisableOnHidden) gameObject.SetActive(false);
            Hidden?.Invoke();
        }

        private void AnimateOnShowing(TweenCallback onComplete)
        {
            canvasGroup.blocksRaycasts = true;
            var tween = showAnim();
            if (tween == null)
            {
                onComplete();
                return;
            }
            hideTween?.Kill();
            hideTween = null;
            tween.OnComplete(onComplete).OnRewind(onComplete);
            if (tween.Duration() == 0)
            {
                onComplete();
            }
            else
            {
                showTween = tween;
            }
        }

        private void AnimateOnHiding(TweenCallback onComplete)
        {
            if (needDisableOnHidden)
            {
                canvasGroup.blocksRaycasts = false;
            }
            
            var tween = hideAnim();
            if (tween == null)
            {
                onComplete();
                return;
            }
            showTween?.Kill();
            showTween = null;
            tween.OnComplete(onComplete).OnRewind(onComplete);
            if (tween.Duration() == 0)
            {
                onComplete();
            }
            else
            {
                hideTween = tween;
            }
        }
    }
}