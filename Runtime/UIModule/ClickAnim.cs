using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

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
    public abstract class BaseSubmittableSelectBehaviour : BaseSubmittableHandler{}

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
        private Vector3 targetScale;
        private Vector3 scaleModification;
        private bool isJustSubmitted;
        
        protected override void Init()
        {
            transform = Submittable.Transform;
            defaultScale = transform.localScale;
            targetScale = defaultScale;
            Submittable.States.PressChanged += OnPress;
            Submittable.States.HoverChanged += OnHover;
            Submittable.States.SelectChanged += OnSelect;
            Submittable.Submitted += OnSubmit;
        }

        private void OnSelect(bool isSelected)
        {
            if (isSelected)
            {
                scaleModification = defaultScale * 0.1f;
            }
            else
            {
                scaleModification = Vector3.zero;
            }

            AnimScale(0.15f);
        }

        private void OnHover(bool isHovering)
        {
            if(isHovering)
            {
                if (Submittable.States.Press)
                {
                    OnPress(true);
                    return;
                }
                
                targetScale = defaultScale * 1.1f;
            }
            else
            {
                targetScale = defaultScale;
                
                if (isJustSubmitted)
                {
                    return;
                }
            }
            
            AnimScale(0.15f);
        }

        private void OnPress(bool isPressing)
        {
            current.Kill();
            float duration;
            if (isPressing)
            {
                duration = 0.3f;
                targetScale = defaultScale * 0.8f;
            }
            else
            {
                duration = 0.15f;
                targetScale = defaultScale;
            }

            AnimScale(duration);
        }
        
        private void OnSubmit()
        {
            targetScale = Submittable.States.Hover ? defaultScale * 1.1f : defaultScale;
            AnimScale(0.5f).SetEase(Ease.OutElastic);
            isJustSubmitted = true;
            EventSystem.Updated += OnUpdate;


            void OnUpdate()
            {
                EventSystem.Updated -= OnUpdate;
                isJustSubmitted = false;
            }
        }

        public override void OnDisable()
        {
            current.Kill();
            if (current == null) return;
            transform.localScale = defaultScale;
            current = null;
        }
        
        private Tween AnimScale(float duration)
        {
            current.Kill();
            current = transform.DOScale(targetScale + scaleModification, duration);
            return current;
        }
    }
}