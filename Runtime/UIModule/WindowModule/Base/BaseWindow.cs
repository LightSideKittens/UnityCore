using System;
using DG.Tweening;
using LSCore.Extensions.Unity;
using Sirenix.OdinInspector;
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
        
        private static bool isCalledFromStatic;
        
        [SerializeField] protected float animDuration = 0.2f;
        private CanvasGroup canvasGroup;
        
        public RectTransform RectTransform { get; private set; }
        public Canvas Canvas { get; private set; }
        protected virtual WindowManager Manager { get; } = new();

        [field: Header("Optional")]
        [field: SerializeField] protected virtual LSButton HomeButton { get; private set; }
        [field: SerializeField] protected virtual LSButton BackButton { get; private set; }

        protected virtual ShowWindowOption ShowOption { get; private set; }
        protected virtual RectTransform Daddy => DaddyCanvas.IsExistsInManager ? DaddyCanvas.Instance.RectTransform : null;
        protected virtual float DefaultAlpha => 0;
        protected virtual bool ActiveByDefault => false;


        protected override void Init()
        {
            base.Init();
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = DefaultAlpha;
            
            var canvas = GetComponent<Canvas>();
            Canvas = canvas;
            var rectTransform = (RectTransform)transform;
            RectTransform = rectTransform;

            InitManager();
            
            if (Daddy != null)
            {
                transform.SetParent(Daddy, false);
                FitInSafeArea(rectTransform);
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

        protected virtual void InitManager()
        {
            Manager.Init(gameObject, Canvas);
            Manager.Showing += OnShowing;
            Manager.Showed += OnShowed;
            Manager.Hiding += OnHiding;
            Manager.Hidden += OnHidden;
            Manager.showOption = () => ShowOption;
            Manager.showAnim = () => ShowAnim;
            Manager.hideAnim = () => HideAnim;
        }

        [Button]
        private void FitInSafeArea()
        {
            FitInSafeArea((RectTransform)transform);
        }
        
        private void FitInSafeArea(RectTransform target)
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
        
        protected override void DeInit()
        {
            base.DeInit();
            isCalledFromStatic = false;
        }

        protected virtual void OnShowing() { }
        protected virtual void OnHiding() { }
        protected virtual void OnShowed() {}
        protected virtual void OnHidden() { }
        protected virtual Tween ShowAnim => canvasGroup.DOFade(1, animDuration);

        protected virtual Tween HideAnim => canvasGroup.DOFade(0, animDuration);
        
        protected virtual void OnBackButton() => WindowsData.GoBack();

        protected virtual void OnHomeButton() => WindowsData.GoHome();
        
        public static void AsHome() => WindowsData.SetHome(Instance.Manager);
        
        public static void Show(ShowWindowOption option)
        {
            isCalledFromStatic = true;
            Instance.ShowOption = option;
            Show();
        }

        public static void Show()
        {
            isCalledFromStatic = true;
            Instance.Manager.Show();
        }

        internal static void GoHome()
        {
            Instance.Manager.GoHome();
        }
    }
}
