using System;
using DG.Tweening;
using LSCore.Extensions.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace LSCore.AnimationsModule.Animations
{
    [Serializable]
    public class AlphaAnim : BaseAnim<float>
    {
        public bool useCanvasGroup;
        
        [HideIf(nameof(useCanvasGroup))]
        public Graphic target;
        
        [ShowIf(nameof(useCanvasGroup))]
        public CanvasGroup canvasGroup;

        protected override void Internal_Init()
        {
            if (useCanvasGroup)
            {
                canvasGroup.alpha = startValue;
            }
            else
            {
                target.A(startValue);
            }
            
        }

        protected override Tween Internal_Animate()
        {
            if (useCanvasGroup)
            {
                return canvasGroup.DOFade(endValue, duration);
            }
            
            return target.DOFade(endValue, duration);
        }
    }
}