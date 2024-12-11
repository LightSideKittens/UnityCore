using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace LSCore
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class BaseWindow<T> : BaseCanvasView<T> where T : BaseWindow<T>
    {
#pragma warning disable CS0067
        public static event Action Showing;
        public static event Action Hiding;

        public static event Action Showed;
        public static event Action Hidden;
#pragma warning restore CS0067
        
        private static bool isCalledFromStatic;
        
        protected override void Init()
        {
            base.Init();
            if(isCalledFromStatic) return;
            gameObject.SetActive(ActiveByDefault);
        }

        protected override void DeInit()
        {
            base.DeInit();
            isCalledFromStatic = false;
        }
        
        private static string logTag = $"[{typeof(T).Name}]".ToTag(new Color(0f, 0.79f, 0.22f));
        public new static void AsHome() => WindowsData.SetHome(Instance.Manager);
        
        [Preserve]
        public new static void Show(ShowWindowOption option)
        {
            Burger.Log($"{logTag} Show {option}");
            isCalledFromStatic = true;
            Instance.ShowOption = option;
            Show();
        }

        public new static void Show()
        {
            Burger.Log($"{logTag} Show");
            isCalledFromStatic = true;
            Instance.Manager.Show();
        }
    }
}
