using System;
using System.Collections.Generic;
using DG.Tweening;
using LSCore.AnimationsModule.Animations.Options;
using LSCore.Extensions;
using LSCore.Extensions.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LSCore.AnimationsModule.Animations
{
    [Serializable]
    public abstract class BaseAnim
    { 
        [BoxGroup] [LabelText("ID")] public string id;
        [HideIf("IsDurationZero")]
        [SerializeReference] private List<IOption> mainOptions;

        public abstract bool NeedInit { get; set; }
        public abstract float Duration { get; set; }
        
        public Tween Anim { get; private set; }
        public bool IsDurationZero => Duration == 0;
        private bool HideDuration => GetType() == typeof(AlphaAnim);

        public void TryInit()
        {
            if (NeedInit || IsDurationZero)
            {
                Internal_Init();
            }
        }

        protected abstract void Internal_Init();
        
        protected abstract Tween Internal_Animate();
        
        public Tween Animate()
        {
            TryInit();
            Anim = IsDurationZero ? DOTween.Sequence() : ApplyOptions(Internal_Animate(), mainOptions);
            return Anim;
        }

        protected static Tween ApplyOptions(Tween tween, List<IOption> options)
        {
            if (options is {Count: > 0})
            {
                for (int i = 0; i < options.Count; i++)
                {
                    options[i].ApplyTo(tween);
                }
            }

            return tween;
        }
    }
    
    [Serializable]
    public abstract class BaseAnim<T, TTarget> : BaseAnim where TTarget : Object
    {
        [field: SerializeField] public override bool NeedInit { get; set; }
        [field: HideIf("HideDuration"), SerializeField] public override float Duration { get; set; }
        
        [ShowIf("ShowStartValue")] public T startValue;
        [ShowIf("ShowEndValue")] public T endValue;

        [HideIf("@IsDurationZero || !UseMultiple")]
        [SerializeReference] public List<IOption> options;

        [SerializeReference] public List<Get<TTarget>> targets = new();

        [ShowIf("UseMultiple")] public AnimationCurve timeOffsetPerTarget = AnimationCurve.Constant(0, 0, 0.1f);

        public bool UseMultiple => targets.Count > 1;
        public TTarget FirstTarget
        {
            get => targets[0];
            set => targets[0] = new SerializeField<TTarget> { data = value };
        }

        public List<Tween> Tweens { get; private set; }
        protected virtual bool ShowStartValue => NeedInit;
        protected virtual bool ShowEndValue => !IsDurationZero;
        
        protected abstract void InitAction(TTarget target);
        protected abstract Tween AnimAction(TTarget target);
        
        protected override void Internal_Init()
        {
            if (UseMultiple)
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    InitAction(targets[i]);
                }
                
                return;
            }
            
            InitAction(targets[0]);
        }

        protected override Tween Internal_Animate()
        {
            Tweens ??= new();
            Tweens.Clear();
            
            if (UseMultiple)
            {
                var sequence = DOTween.Sequence();
                var pos = 0f;
                var timeOffset = timeOffsetPerTarget.length == 0 ? 0 : timeOffsetPerTarget[timeOffsetPerTarget.length - 1].time / (targets.Count - 1);
                for (int i = 0; i < targets.Count; i++)
                {
                    var t = ApplyOptions(AnimAction(targets[i]), options);
                    t.KillOnDestroy();
                    Tweens.Add(t);
                    sequence.Insert(pos, t);
                    pos += timeOffsetPerTarget.Evaluate((i + 1) * timeOffset);
                }

                return sequence;
            }
            
            return ApplyOptions(AnimAction(targets[0]).KillOnDestroy(), options);
        }

        public void Reverse()
        {
            (startValue, endValue) = (endValue, startValue);
        }
    }
}