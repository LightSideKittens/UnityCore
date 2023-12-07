using UnityEngine;

namespace LightSideCore.Runtime.UIModule.TabAnimations
{
    public abstract class BaseTabAnim
    {
        public float duration = 0.2f;
        [HideInInspector] public CanvasGroup group;
        [HideInInspector] public RectTransform parent;
        [HideInInspector] public RectTransform reference;
        
        public abstract void ShowAnim();
        public abstract void HideAnim();
    }
}