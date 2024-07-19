using System;
using DG.Tweening;
using LSCore.AnimationsModule.Animations.Options;
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
        [SerializeReference] private IOptions[] options;

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
    public abstract class BaseAnim<T, TTarget> : BaseAnim, ISerializationCallbackReceiver where TTarget : Object
    {
        [field: HideIf("IsDurationZero")]
        [field: SerializeField] public override bool NeedInit { get; set; }

        [field: HideIf("HideDuration")]
        [field: SerializeField] public override float Duration { get; set; }
        
        [ShowIf("ShowStartValue")]
        public T startValue;
        
        [ShowIf("ShowEndValue")]
        public T endValue;
        
        public bool useTargetPath;
        public bool useMultiple;
        
        [HideIf("useTargetPath")] public TTarget target;
        [ShowIf("ShowTargets")] public TTarget[] targets;
        
        [ShowIf("useTargetPath")] public Transform root;
        [ShowIf("useTargetPath")] public string targetPath;
        [ShowIf("ShowTargetsPaths")] public string[] targetsPaths;
        
        [ShowIf("useMultiple")] public float timeOffsetPerTarget = 0.1f;

        private bool ShowTargets => useMultiple && !useTargetPath;
        private bool ShowTargetsPaths => useMultiple && useTargetPath;
        protected virtual bool ShowStartValue => NeedInit;
        protected virtual bool ShowEndValue => !IsDurationZero;
        
        protected abstract void InitAction(TTarget target);
        protected abstract Tween AnimAction(TTarget target);
        
        protected override void Internal_Init()
        {
            InitAction(target);
            for (int i = 0; i < targets.Length; i++)
            {
                InitAction(targets[i]);
            }
        }

        protected override Tween Internal_Animate()
        {
            if (useMultiple)
            {
                var sequence = DOTween.Sequence();
                var pos = 0f;
                sequence.Insert(pos, AnimAction(target));
                for (int i = 0; i < targets.Length; i++)
                {
                    pos += timeOffsetPerTarget;
                    sequence.Insert(pos, AnimAction(targets[i]));
                }

                return sequence;
            }
            
            return AnimAction(target);
        }

        public void Reverse()
        {
            (startValue, endValue) = (endValue, startValue);
        }

        public virtual void OnBeforeSerialize() { }

        public virtual void OnAfterDeserialize()
        {
            if(!World.IsPlaying) return;
            if (useTargetPath)
            {
                if (typeof(Component).IsAssignableFrom(typeof(T)))
                {
                    target = root.FindComponent<TTarget>(targetPath);
                    targets = new TTarget[targetsPaths.Length];

                    for (int i = 0; i < targetsPaths.Length; i++)
                    {
                        targets[i] = root.FindComponent<TTarget>(targetsPaths[i]);
                    }
                }
            }
        }
    }
}