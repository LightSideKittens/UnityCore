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
    public class DefaultSubmittableSelectBehaviour : BaseSubmittableSelectBehaviour
    {
        private Transform transform;
        protected override void Init()
        {
            transform = Submittable.Transform;
        }

        public override void OnDisable()
        {
            throw new NotImplementedException();
        }
        
        /*public Selectable FindSelectable(Vector3 dir)
        {
            dir = dir.normalized;
            Vector3 localDir = Quaternion.Inverse(transform.rotation) * dir;
            Vector3 pos = transform.TransformPoint(GetPointOnRectEdge(transform as RectTransform, localDir));
            float maxScore = Mathf.NegativeInfinity;
            float maxFurthestScore = Mathf.NegativeInfinity;
            float score = 0;

            bool wantsWrapAround = navigation.wrapAround && (m_Navigation.mode == Navigation.Mode.Vertical || m_Navigation.mode == Navigation.Mode.Horizontal);

            Selectable bestPick = null;
            Selectable bestFurthestPick = null;

            for (int i = 0; i < s_SelectableCount; ++i)
            {
                Selectable sel = s_Selectables[i];

                if (sel == this)
                    continue;

                if (!sel.IsInteractable() || sel.navigation.mode == Navigation.Mode.None)
                    continue;

#if UNITY_EDITOR
                if (Camera.current != null && !UnityEditor.SceneManagement.StageUtility.IsGameObjectRenderedByCamera(sel.gameObject, Camera.current))
                    continue;
#endif
                
                var selRect = sel.transform as RectTransform;
                Vector3 selCenter = selRect != null ? (Vector3)selRect.rect.center : Vector3.zero;
                Vector3 myVector = sel.transform.TransformPoint(selCenter) - pos;

                float dot = Vector3.Dot(dir, myVector);

                if (wantsWrapAround && dot < 0)
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

            if (wantsWrapAround && null == bestPick) return bestFurthestPick;

            return bestPick;
        }*/
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