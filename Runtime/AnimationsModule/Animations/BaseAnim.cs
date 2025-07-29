using System;
using System.Collections.Generic;
using DG.Tweening;
using LSCore.AnimationsModule.Animations.Options;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LSCore.AnimationsModule.Animations
{
    [Serializable]
    public abstract class BaseAnim
    { 
        [BoxGroup] [LabelText("ID")] public string id;
        public abstract bool NeedInit { get; }
        public Tween Anim { get; private set; }

        public void TryInit()
        {
            if (NeedInit)
            {
                Internal_Init();
            }
        }

        protected abstract void Internal_Init();
        
        protected abstract Tween Internal_Animate();
        
        public Tween Animate()
        {
            TryInit();
            Anim = Internal_Animate();
            return Anim;
        }

        public static Tween ApplyOptions(Tween tween, List<IOption> options)
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
    public abstract class SingleAnim<TTarget> : BaseAnim where TTarget : Object
    {
        [SerializeReference] public List<IOption> options;
        [SerializeReference] [LabelText("$TargetsLabel")] public List<Get<TTarget>> targets = new();
        [ShowIf("UseMultiple")] public AnimationCurve timeOffsetPerTarget = AnimationCurve.Constant(0, 0, 0.1f);

        private string TargetsLabel => $"{typeof(TTarget).Name}s";
        
        public override bool NeedInit => false;
        public bool UseMultiple => targets.Count > 1;
        public TTarget FirstTarget
        {
            get => targets[0];
            set => targets[0] = new SerializeField<TTarget> { data = value };
        }

        public List<Tween> Tweens { get; protected set; }
        
        protected abstract Tween AnimAction(TTarget target);

        protected override void Internal_Init() { }

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
                    float interval = timeOffsetPerTarget.Evaluate((i + 1) * timeOffset);
                    
                    if (t.Loops() == -1)
                    {
                        sequence.AppendInterval(interval);
                        t.SetDelay(interval);
                    }
                    else
                    { 
                        sequence.Insert(pos, t);
                    }
                    
                    pos += interval;
                }

                return sequence;
            }
            
            return ApplyOptions(AnimAction(targets[0]).KillOnDestroy(), options);
        }
    }
    
    [Serializable]
    public abstract class BaseAnim<TValue, TTarget> : SingleAnim<TTarget> where TTarget : Object
    {
        public bool needInit;
        public float duration;
        public override bool NeedInit => needInit;
        
        [ShowIf("ShowStartValue")] public TValue startValue;
        [ShowIf("ShowEndValue")] public TValue endValue;
        
        protected virtual bool ShowStartValue => NeedInit;
        protected virtual bool ShowEndValue => duration != 0;
        
        protected abstract void InitAction(TTarget target);
        
        protected override void Internal_Init()
        {
            for (int i = 0; i < targets.Count; i++)
            {
                InitAction(targets[i]);
            }
        }

        public void Reverse()
        {
            (startValue, endValue) = (endValue, startValue);
        }

        public void SetSatrtValue(Func<TValue> getter, Action<TValue> setter, Func<TValue, TValue, TValue> modificator)
        {
            for (var i = 0; i < options.Count; i++)
            {
                var option = options[i];
                if (option is SetRelative)
                {
                    setter(modificator(startValue, getter()));
                    return;
                }
            }
            
            setter(getter());
        }
    }
}