using System;
using DG.Tweening;
using LSCore.Extensions.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace LSCore.AnimationsModule.Animations
{
    [Serializable]
    public class AlphaAnim : BaseAnim
    {
        [SerializeReference] public CanvasGroupAlphaAnim canvasGroupAnim;
        [SerializeReference] public GraphicAlphaAnim graphicAnim;

        public override bool NeedInit
        {
            get
            {
                if (canvasGroupAnim != null)
                {
                    return canvasGroupAnim.needInit;
                }

                if (graphicAnim != null)
                {
                    return graphicAnim.needInit;
                }

                return false;
            }
        }

        protected override void Internal_Init()
        {
            canvasGroupAnim?.TryInit();
            graphicAnim?.TryInit();
        }

        protected override Tween Internal_Animate()
        {
            if (canvasGroupAnim != null && graphicAnim != null)
            {
                var sequence = DOTween.Sequence().Insert(0, canvasGroupAnim.Animate()).Insert(0, graphicAnim.Animate());
                return sequence;
            }
            
            if(canvasGroupAnim != null)
            {
                return canvasGroupAnim.Animate();
            }
            
            if(graphicAnim != null)
            {
                return graphicAnim.Animate();
            }

            throw new NullReferenceException();
        }
    }
    
    [Serializable]
    public class CanvasGroupAlphaAnim : BaseAnim<float, CanvasGroup>
    {
        protected override void InitAction(CanvasGroup target)
        {
            target.alpha = startValue;
        }

        protected override Tween AnimAction(CanvasGroup target)
        {
            return target.DOFade(endValue,duration);
        }
    }
    
    [Serializable]
    public class GraphicAlphaAnim : BaseAnim<float, Graphic>
    {
        protected override void InitAction(Graphic target)
        {
            target.A(startValue);
        }

        protected override Tween AnimAction(Graphic target)
        {
            return target.DOFade(endValue,duration);
        }
    }
}