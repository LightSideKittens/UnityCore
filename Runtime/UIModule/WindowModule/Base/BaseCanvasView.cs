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
                WindowManager.FitInSafeArea(RectTransform);
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
            WindowManager.FitInSafeArea((RectTransform)transform);
        }
    }
}