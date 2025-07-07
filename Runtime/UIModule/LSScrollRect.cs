using System;
using DG.Tweening;
using LSCore.AnimationsModule.Animations;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace LSCore
{
    [Serializable]
    public class ScrollToAnim : SingleAnim<ScrollRect>
    {
        public float duration;
        [SerializeReference] public Get<RectTransform> target;
        
        protected override Tween AnimAction(ScrollRect target)
        {
            return target.content.DOLocalMove(target.GetGoToPos(this.target), duration);
        }

        private Vector3 GetCenter(RectTransform rt)
        {
            Vector2 localCenter = rt.rect.center;
            return rt.TransformPoint(localCenter);
        }
    }
    
    public class LSScrollRect : ScrollRect
    {
        public ScrollToAnim scrollToAnim;
        private Tween tween;
        
        public override void GoTo(RectTransform target)
        {
            tween?.Kill();
            scrollToAnim.target = target;
            tween = scrollToAnim.Animate();
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(LSScrollRect), true)]
    [CanEditMultipleObjects]
    /// <summary>
    /// Custom Editor for the ScrollRect Component.
    /// Extend this class to write a custom editor for a component derived from ScrollRect.
    /// </summary>
    public class LSScrollRectEditor : ScrollRectEditor
    {
        private PropertyTree propertyTree;
        private InspectorProperty scrollToAnim;

        protected override void OnEnable()
        {
            base.OnEnable();
            propertyTree = PropertyTree.Create(serializedObject);
            scrollToAnim = propertyTree.RootProperty.Children["scrollToAnim"];
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            propertyTree?.Dispose();
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            propertyTree.BeginDraw(true);
            scrollToAnim.Draw();
            propertyTree.EndDraw();
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}