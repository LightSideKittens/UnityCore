using System;
using DG.Tweening;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public abstract class BaseSubmittableHandler
    {
        public ISubmittable Submittable { get; private set; }

        public void Init(ISubmittable submittable)
        {
            Submittable = submittable;
            Init();
        }
        
        protected abstract void Init();
        public abstract void OnDisable();
    }
    
    [Serializable]
    public abstract class BaseSubmittableAnim : BaseSubmittableHandler{}
    [Serializable]
    public abstract class BaseSubmittableDoIter : BaseSubmittableHandler{}

    [Serializable]
    public class DefaultSubmittableDoIter : BaseSubmittableDoIter
    {
        [SerializeReference] public DoIt[] onSubmit;
        
        protected override void Init()
        {
            Submittable.Submitted += onSubmit.Do;
        }

        public override void OnDisable() { }
    }

    [Serializable]
    public class DefaultSubmittableAnim : BaseSubmittableAnim
    {
        private Transform transform;
        private Tween current;
        private Vector3 defaultScale;
        
        protected override void Init()
        {
            transform = Submittable.Transform;
            Submittable.States.PressChanged += OnPress;
            Submittable.States.HoverChanged += OnHover;
            Submittable.Submitted += OnSubmit;
        }

        private void OnHover(bool isHovering)
        {
            if(isHovering) OnEnter();
            else OnExit();
        }

        private void OnPress(bool isPressing)
        {
            if(isPressing) OnDown();
            else OnUp();
        }

        private void OnDown()
        {
            current.Kill();
            current = transform.DOScale(defaultScale * 0.8f, 0.3f);
        }
        
        private void OnSubmit()
        {
            current.Kill();
            current = transform.DOScale(defaultScale, 0.5f).SetEase(Ease.OutElastic);
        }
        
        private void OnUp()
        {
            current.Kill();
            current = transform.DOScale(defaultScale, 0.15f);
        }

        private void OnEnter()
        {
            current.Complete();
            defaultScale = transform.localScale;
            current = transform.DOScale(defaultScale * 1.1f, 0.15f);
        }
        
        private void OnExit()
        {
            current.Kill();
            current = transform.DOScale(defaultScale, 0.15f);
        }

        public override void OnDisable()
        {
            current.Kill();
            if (current == null) return;
            transform.localScale = defaultScale;
            current = null;
        }
    }
}