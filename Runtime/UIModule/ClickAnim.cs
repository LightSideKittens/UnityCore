using System;
using System.Collections.Generic;
using DG.Tweening;
using LSCore.Extensions.Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        
        protected virtual void Init(){}
        public virtual void OnEnable(){}
        public virtual void OnDisable(){}
    }
    
    [Serializable]
    public abstract class BaseSubmittableAnim : BaseSubmittableHandler{}
    [Serializable]
    public abstract class BaseSubmittableDoIter : BaseSubmittableHandler{}

    [Serializable]
    public abstract class BaseSubmittableSelectBehaviour : BaseSubmittableHandler
    {
        public abstract void OnMove(AxisEventData eventData);
    }

    [Serializable]
    public class DefaultSubmittableSelectBehaviour : BaseSubmittableSelectBehaviour
    {
        private Transform transform;
        private static HashSet<ISubmittable> selectables = new();
        public bool selectOnPress = true;
        public bool selectOnHover;
        
        protected override void Init()
        {
            transform = Submittable.Transform;
            if (selectOnPress)
            {
                Submittable.States.PressChanged += OnPress;
            }

            if (selectOnHover)
            {
                Submittable.States.PressChanged += OnPress;
            }
            
            Submittable.States.SelectChanged += OnSelect;
        }
        

        private void OnPress(bool isPressing)
        {
            if (isPressing)
            {
                EventSystem.current.SetSelectedGameObject(transform.gameObject, Submittable.States.currentEventData);
            }
        }
        
        private void OnHover(bool isHovering)
        {
            if (isHovering)
            {
                EventSystem.current.SetSelectedGameObject(transform.gameObject, Submittable.States.currentEventData);
            }
        }

        private void OnSelect(bool isSelected)
        {
            if (isSelected)
            {
                var scrollRect = transform.GetComponentInParent<ScrollRect>();
                if (scrollRect)
                {
                    scrollRect.GoTo(transform as RectTransform);
                }
            }
        }

        public override void OnEnable()
        {
            selectables.Add(Submittable);
        }

        public override void OnDisable()
        {
            selectables.Remove(Submittable);
        }

        public override void OnMove(AxisEventData eventData)
        {
            var rotation = transform.rotation;
            switch (eventData.moveDir)
            {
                case MoveDirection.Right:
                    Navigate(eventData, FindSelectable(rotation * Vector3.right));
                    break;

                case MoveDirection.Up:
                    Navigate(eventData, FindSelectable(rotation * Vector3.up));
                    break;

                case MoveDirection.Left:
                    Navigate(eventData, FindSelectable(rotation * Vector3.left));
                    break;

                case MoveDirection.Down:
                    Navigate(eventData, FindSelectable(rotation * Vector3.down));
                    break;
            }
        }
        
        void Navigate(AxisEventData eventData, ISubmittable sel)
        {
            eventData.selectedObject = sel.Transform.gameObject;
        }

        public ISubmittable FindSelectable(Vector3 dir)
        {
            dir = dir.normalized;
            Vector3 localDir = Quaternion.Inverse(transform.rotation) * dir;
            Vector3 pos = transform.TransformPoint(GetPointOnRectEdge(transform as RectTransform, localDir));
            float maxScore = Mathf.NegativeInfinity;
            float maxFurthestScore = Mathf.NegativeInfinity;
            float score = 0;

            ISubmittable bestPick = null;
            ISubmittable bestFurthestPick = null;

            foreach (var sel in selectables)
            {
                if (sel == Submittable)
                    continue;
                
                var selRect = sel.Transform as RectTransform;
                Vector3 selCenter = selRect != null ? selRect.rect.center : Vector3.zero;
                Vector3 myVector = sel.Transform.TransformPoint(selCenter) - pos;

                float dot = Vector3.Dot(dir, myVector);

                if (dot < 0)
                {
                    score = -dot * myVector.sqrMagnitude;

                    if (score > maxFurthestScore)
                    {
                        maxFurthestScore = score;
                        bestFurthestPick = sel;
                    }

                    continue;
                }

                if (dot <= 0)
                    continue;

                score = dot / myVector.sqrMagnitude;

                if (score > maxScore)
                {
                    maxScore = score;
                    bestPick = sel;
                }
            }

            if (null == bestPick) return bestFurthestPick;

            return bestPick;
        }
        
        private static Vector3 GetPointOnRectEdge(RectTransform rect, Vector2 dir)
        {
            if (rect == null)
                return Vector3.zero;
            if (dir != Vector2.zero)
                dir /= Mathf.Max(Mathf.Abs(dir.x), Mathf.Abs(dir.y));
            dir = rect.rect.center + Vector2.Scale(rect.rect.size, dir * 0.5f);
            return dir;
        }
    }

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
        
        protected override void Init()
        {
            transform = Submittable.Transform;
            defaultScale = transform.localScale;
            targetScale = defaultScale;
            Submittable.States.PressChanged += OnPress;
            Submittable.Submitted += OnSubmit;
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
            targetScale = defaultScale;
            AnimScale(0.5f).SetEase(Ease.OutElastic);
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
    
    [Serializable]
    public class StandaloneSubmittableAnim : BaseSubmittableAnim
    {
        private Transform transform;
        private Tween current;
        private Vector3 defaultScale;
        private Vector3 targetScale;
        private Vector3 scaleModification;
        
        protected override void Init()
        {
            transform = Submittable.Transform;
            defaultScale = transform.localScale;
            targetScale = defaultScale;
            Submittable.States.HoverChanged += OnHover;
            Submittable.States.PressChanged += OnPress;
            Submittable.States.SelectChanged += OnSelect;
            Submittable.Submitted += OnSubmit;
        }

        private void OnSelect(bool isSelected)
        {
            float duration = 0.3f;
            scaleModification += defaultScale * (0.1f * (isSelected ? 1 : -1));
            AnimScale(duration);
        }

        private void OnPress(bool isPressing)
        {
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
        
        private void OnHover(bool isHovering)
        {
            float duration = 0.15f;
            scaleModification += defaultScale * (0.1f * (isHovering ? 1 : -1));
            AnimScale(duration);
        }
        
        private void OnSubmit()
        {
            targetScale = defaultScale;
            AnimScale(0.5f).SetEase(Ease.OutElastic);
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