using System;
using DG.Tweening;
using UnityEngine;

namespace LSCore.AnimationsModule.Animations
{
    [Serializable]
    public class GOsActiveAnim : BaseAnim<bool>
    {
        public GameObject[] target;

        protected override void Internal_Init()
        {
            for (int i = 0; i < target.Length; i++)
            {
                target[i].SetActive(startValue);
            }
        }
        
        protected override Tween Internal_Animate()
        {
            return DOTween.Sequence().AppendInterval(duration).AppendCallback(() =>
            {
                for (int i = 0; i < target.Length; i++)
                {
                    target[i].SetActive(endValue);
                }
            });
        }
    }
}