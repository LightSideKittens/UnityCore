using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LSCore
{
    [Serializable]
    public abstract class BaseUIControlSelectBehaviour : BaseUIControlHandler
    {
        public abstract void OnMove(AxisEventData eventData);
    }
    
    [Serializable]
    public class DefaultUIControlSelectBehaviour : BaseUIControlSelectBehaviour
    {
        private Transform transform;
        private static HashSet<IUIControl> selectables = new();
        public bool selectOnPress = true;
        public bool selectOnHover;
        
        protected override void Init()
        {
            transform = UIControl.Transform;
            if (selectOnPress)
            {
                UIControl.States.PressChanged += OnPress;
            }

            if (selectOnHover)
            {
                UIControl.States.PressChanged += OnPress;
            }
            
            UIControl.States.SelectChanged += OnSelect;
        }
        

        private void OnPress(bool isPressing)
        {
            if (isPressing)
            {
                EventSystem.current.SetSelectedGameObject(transform.gameObject, UIControl.States.currentEventData);
            }
        }
        
        private void OnHover(bool isHovering)
        {
            if (isHovering)
            {
                EventSystem.current.SetSelectedGameObject(transform.gameObject, UIControl.States.currentEventData);
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
            selectables.Add(UIControl);
        }

        public override void OnDisable()
        {
            selectables.Remove(UIControl);
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

        private void Navigate(AxisEventData eventData, IUIControl sel)
        {
            eventData.selectedObject = sel.Transform.gameObject;
        }

        public IUIControl FindSelectable(Vector3 dir)
        {
            dir = dir.normalized;
            var localDir = Quaternion.Inverse(transform.rotation) * dir;
            var pos = transform.TransformPoint(GetPointOnRectEdge(transform as RectTransform, localDir));
            var maxScore = Mathf.NegativeInfinity;
            var maxFurthestScore = Mathf.NegativeInfinity;

            IUIControl bestPick = null;
            IUIControl bestFurthestPick = null;

            foreach (var sel in selectables)
            {
                if (sel == UIControl)
                    continue;
                
                var selRect = sel.Transform as RectTransform;
                Vector3 selCenter = selRect != null ? selRect.rect.center : Vector3.zero;
                var myVector = sel.Transform.TransformPoint(selCenter) - pos;

                var dot = Vector3.Dot(dir, myVector);

                float score;
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
}