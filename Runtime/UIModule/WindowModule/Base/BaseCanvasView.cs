using LSCore.Extensions.Unity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore
{
    [RequireComponent(typeof(Canvas))]
    public abstract class BaseCanvasView<T> : BaseUIView<T> where T : BaseCanvasView<T>
    {
        protected virtual RectTransform Daddy => DaddyCanvas.IsExistsInManager ? DaddyCanvas.Instance.RectTransform : null;
        public Canvas Canvas { get; private set; }

        protected override void Init()
        {
            var canvas = GetComponent<Canvas>();
            Canvas = canvas;
            base.Init();

            if (Daddy != null)
            {
                transform.SetParent(Daddy, false);
                FitInSafeArea(RectTransform);
            }
            else
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = Camera.main;
            }

            canvas.overrideSorting = true;
        }

        protected override void InitManager()
        {
            base.InitManager();
            Manager.canvas = Canvas;
        }
        
        [Button]
        private void FitInSafeArea()
        {
            FitInSafeArea((RectTransform)transform);
        }
        
        private static void FitInSafeArea(RectTransform target)
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
            target.localScale = one;

            target.offsetMin = safeArea.min * xFactor;
            target.offsetMax = (safeArea.max - new Vector2(LSScreen.Width, LSScreen.Height)) * yFactor;
        }

    }
}