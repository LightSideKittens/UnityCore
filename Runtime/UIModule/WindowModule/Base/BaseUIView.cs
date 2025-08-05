using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Scripting;

namespace LSCore
{
    public interface IUIView
    {
        RectTransform RectTransform { get; }
        WindowManager Manager { get; }
        
        static Action<IUIView> Showing; 
        static Action<IUIView> Showed; 
        static Action<IUIView> Hiding;
        static Action<IUIView> Hidden;
    }
    
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(RectTransform))]
    public abstract class BaseUIView<T> : SingleService<T>, IUIView where T : BaseUIView<T>
    {
        [SerializeReference] public ShowHideAnim showHideAnim = new DefaultUIViewAnimation();
        public RectTransform RectTransform { get; private set; }
        public virtual WindowManager Manager { get; } = new();
        
        [SerializeReference, FoldoutGroup("Optional")] protected List<DoIt> onShowing;
        [SerializeReference, FoldoutGroup("Optional")] protected List<DoIt> onShowed;
        [SerializeReference, FoldoutGroup("Optional")] protected List<DoIt> onHiding;
        [SerializeReference, FoldoutGroup("Optional")] protected List<DoIt> onHidden;
        

        protected virtual ShowWindowOption ShowOption { get; set; }
        protected virtual bool ActiveByDefault => false;

        [Preserve]
        protected void SetAnimation(ShowHideAnim anim) => showHideAnim = anim;
        [Preserve]
        protected void SetNeedDisableOnHidden(bool needDisableOnHidden) => Manager.needDisableOnHidden = needDisableOnHidden;

        protected override void Init()
        {
            base.Init();
            InitManager();
            var rectTransform = (RectTransform)transform;
            RectTransform = rectTransform;
        }

        protected virtual void InitManager()
        {
            Manager.Init(GetComponent<CanvasGroup>());
            Manager.Showing += () => { OnShowing(); IUIView.Showing?.Invoke(this); };
            Manager.Showed += () => { OnShowed(); IUIView.Showed?.Invoke(this); };
            Manager.Hiding += () => { OnHiding(); IUIView.Hiding?.Invoke(this); };
            Manager.Hidden += () => { OnHidden(); IUIView.Hidden?.Invoke(this); };
            Manager.showOption = () => ShowOption;
            Manager.showAnim = () => ShowAnim;
            Manager.hideAnim = () => HideAnim;
        }

        protected virtual void OnShowing() => onShowing.Do();
        protected virtual void OnShowed() => onShowed.Do();
        protected virtual void OnHiding() => onHiding.Do();
        protected virtual void OnHidden() => onHidden.Do();

        protected virtual Tween ShowAnim => showHideAnim?.Show();
        protected virtual Tween HideAnim => showHideAnim?.Hide();

        public void AsHome() => UIViewBoss.SetHome(Manager);

        private static string logTag = $"[{typeof(T).Name}]".ToTag(new Color(0.38f, 1f, 0.33f));
        
        public void Show(ShowWindowOption option)
        {
            Burger.Log($"{logTag} {gameObject.name} Show." +
                       $"\nOption: {option.ToString().ToTag(new Color(0.49f, 0.64f, 1f))}" +
                       $"\nId: {UIViewBoss.Id.ToTag(new Color(0.6f, 0.85f, 0.18f))}");
            ShowOption = option;
            Manager.Show();
        }

        public void Show()
        {
            Burger.Log($"{logTag} {gameObject.name} Show by Id {UIViewBoss.Id}");
            Manager.Show();
        }
    }
}