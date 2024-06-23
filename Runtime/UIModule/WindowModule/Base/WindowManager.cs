using System;
using DG.Tweening;
using UnityEngine;

namespace LSCore
{
    public class WindowManager
    {
        public event Action Showing;
        public event Action Hiding;

        public event Action Showed;
        public event Action Hidden;
        

        private Canvas canvas;
        private Tween showTween;
        private Tween hideTween;
        private GameObject gameObject;

        public Func<ShowWindowOption> showOption;
        public Func<Tween> showAnim;
        public Func<Tween> hideAnim;
        
        public void Init(GameObject gameObject, Canvas canvas = null)
        {
            this.gameObject = gameObject;
            this.canvas = canvas;
        }
        
        public void Show()
        {
            if (WindowsData.IsPreLast(this) && (canvas == null || WindowsData.sortingOrder - 1 > canvas.sortingOrder))
            {
                WindowsData.GoBack();
                return;
            }
            WindowsData.StartRecording();
            InternalShow();
            WindowsData.StopRecording();
        }

        internal void GoHome()
        {
            WindowsData.StartRecording();
            WindowsData.HideAllPrevious();
            Show();
            WindowsData.StopRecording();
        }
        
        private void InternalShow()
        {
            if (showTween != null) return;

            AnimateOnShowing(OnCompleteShow);
            Showing?.Invoke();
            gameObject.SetActive(true);
            WindowsData.CallOption(showOption());
            RecordState();
        }

        protected virtual void RecordState()
        {
            WindowsData.hideAllPrevious += InternalHide;
            WindowsData.Record(InternalHide);
            if (canvas)
            {
                canvas.sortingOrder = WindowsData.sortingOrder++;
            }
        }

        private void InternalHide()
        {
            if (hideTween != null) return;

            AnimateOnHiding(OnCompleteHide);
            if (canvas)
            {
                WindowsData.sortingOrder--;
            }
            WindowsData.hideAllPrevious -= InternalHide;
            WindowsData.Record(InternalShow);
            Hiding?.Invoke();
        }
        
        private void OnCompleteShow()
        {
            Showed?.Invoke();
        }

        private void OnCompleteHide()
        {
            gameObject.SetActive(false);
            Hidden?.Invoke();
        }

        private void AnimateOnShowing(TweenCallback onComplete)
        {
            hideTween?.Kill();
            hideTween = null;
            showTween = showAnim().OnComplete(onComplete);
        }

        private void AnimateOnHiding(TweenCallback onComplete)
        {
            showTween?.Kill();
            showTween = null;
            hideTween = hideAnim().OnComplete(onComplete);
        }
    }
}