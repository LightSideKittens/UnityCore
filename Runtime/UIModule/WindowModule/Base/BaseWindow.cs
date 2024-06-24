using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace LSCore
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class BaseWindow<T> : BaseCanvasView<T> where T : BaseWindow<T>
    {
        public static event Action Showing;
        public static event Action Hiding;

        public static event Action Showed;
        public static event Action Hidden;
        
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
        
        public static void AsHome() => WindowsData.SetHome(Instance.Manager);
        
        [Preserve]
        public static void Show(ShowWindowOption option)
        {
            Burger.Log($"[{typeof(T)}] Show {option}");
            isCalledFromStatic = true;
            Instance.ShowOption = option;
            Show();
        }

        public static void Show()
        {
            Burger.Log($"[{typeof(T)}] Show");
            isCalledFromStatic = true;
            Instance.Manager.Show();
        }
    }
}
