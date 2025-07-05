using System;
using DG.Tweening;
using LSCore.Attributes;
using LSCore.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LSCore
{
    [Unwrap]
    [Serializable]
    public abstract class BaseClickableHandler
    {
        [field: SerializeField] public ClickableStates States { get; private set; } = new();
        public abstract void Init();
        public abstract void OnDown();
        public abstract void OnClick();
        public abstract void OnUp();
        public abstract void OnDisable();
    }
    
    [Unwrap]
    [Serializable]
    public abstract class BaseClickAnim : BaseClickableHandler
    {
        [GetContext] public Object root;
        private Transform transform;
        public Transform Transform => transform ??= root.Cast<Transform>();
        protected Tween current;
        
        public abstract Tween DownTween { get; }
        public abstract Tween UpTween { get; }
        public abstract Tween ClickTween { get; }
        
        public sealed override void OnDown()
        {
            current.Complete();
            current = DownTween;
        }

        public sealed override void OnClick()
        {
            current.Kill();
            current = ClickTween;
        }
        
        public sealed override void OnUp()
        {
            current.Kill();
            current = UpTween;
        }

        public sealed override void OnDisable()
        {
            current.Kill();
            if (current == null) return;
            Reset();
            current = null;
        }

        public abstract void Reset();
    }

    [Serializable]
    public class DefaultScaleClickAnim : BaseClickAnim
    {
        protected Vector3 defaultScale;

        public override Tween DownTween
        {
            get
            {
                defaultScale = Transform.localScale;
                return Transform.DOScale(defaultScale * 0.8f, 0.3f);
            }
        }
        
        public override Tween UpTween => Transform.DOScale(defaultScale, 0.15f);

        public override Tween ClickTween => Transform.DOScale(defaultScale, 0.5f).SetEase(Ease.OutElastic);

        public override void Reset()
        {
            Transform.localScale = defaultScale;
        }
    }
}