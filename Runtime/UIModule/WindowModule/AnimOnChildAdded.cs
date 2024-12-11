using System.Collections.Generic;
using DG.Tweening;
using LSCore.AnimationsModule;
using LSCore.AnimationsModule.Animations;
using UnityEngine;
using UnityEngine.Scripting;

namespace LSCore
{
    [RequireComponent(typeof(ChildrenTracker))]
    public class AnimOnChildAdded : MonoBehaviour
    {
        public new AnimSequencer animation;
        private SizeDeltaAnim sizeAnim;
        private HashSet<IUIView> views = new();
        private ChildrenTracker childrenTracker;
        public float offset = 50;
        
        private void Awake()
        {
            sizeAnim = animation.GetAnim<SizeDeltaAnim>();
            childrenTracker = GetComponent<ChildrenTracker>();
            childrenTracker.Added += t =>
            {
                if (t.TryGetComponent<IUIView>(out var view))
                {
                    views.Add(view);
                    Animate();
                    view.Manager.Showing += Animate;

                    void Animate()
                    {
                        Anim(view.RectTransform.rect.width);
                    }
                }
            };

            foreach (IUIView view in transform)
            {
                Anim(view.RectTransform.rect.width);
                break;
            }
        }

        private void Anim(float width)
        {
            sizeAnim.endValue.x = width + offset;
            animation.Animate();
        }
        
        [Preserve]
        public Tween Hide()
        {
            sizeAnim.endValue.x = 0;
            return animation.Animate();
        }
    }
}