using System;
using DG.Tweening;
using LSCore.AnimationsModule.Animations.Options;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore.AnimationsModule.Animations
{
    [Serializable]
    public abstract class BaseAnim
    {
        [BoxGroup] [LabelText("ID")] public string id;
        [HideIf("IsDurationZero")]
        [SerializeReference] private IOptions[] options;
        [HideIf("IsDurationZero")]
        [SerializeField] private bool needInit;
        public float duration;
        
        public Tween Anim { get; private set; }
        public bool IsDurationZero => duration == 0;

        public void TryInit()
        {
            if (needInit || IsDurationZero)
            {
                Internal_Init();
            }
        }

        protected abstract void Internal_Init();
        
        protected abstract Tween Internal_Animate();
        
        public Tween Animate()
        {
            TryInit();
            Anim = IsDurationZero ? DOTween.Sequence() : ApplyTo(Internal_Animate());
            return Anim;
        }

        protected Tween ApplyTo(Tween tween)
        {
            if (options is {Length: > 0})
            {
                for (int i = 0; i < options.Length; i++)
                {
                    options[i].ApplyTo(tween);
                }
            }

            return tween;
        }
    }
    
    
    [Serializable]
    public abstract class BaseAnim<T> : BaseAnim
    {
        public T startValue;
        
        [HideIf("IsDurationZero")]
        public T endValue;

        public void Reverse()
        {
            (startValue, endValue) = (endValue, startValue);
        }
    }
}