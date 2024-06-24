using DG.Tweening;
using LSCore.AnimationsModule;
using LSCore.AnimationsModule.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace LSCore
{
    public class AnimOnChildAdded : MonoBehaviour
    {
        public AnimSequencer animation;
        private SizeDeltaAnim sizeAnim;
        private float lastSize;
        private HorizontalOrVerticalLayoutGroup layoutGroup;
        
        private void Awake()
        {
            layoutGroup = GetComponent<HorizontalOrVerticalLayoutGroup>();
            sizeAnim = animation.GetAnim<SizeDeltaAnim>();
        }

        private void Update()
        {
            var currentSize = layoutGroup.preferredWidth;
            
            if (Mathf.Abs(lastSize - currentSize) > 1f)
            {
                sizeAnim.endValue.x = currentSize;
                animation.Animate();
            }

            lastSize = currentSize;
        }

        public void Show()
        {
            enabled = true;
        }
        
        public Tween Hide()
        {
            enabled = false;
            sizeAnim.endValue.x = 0;
            lastSize = 0;
            return animation.Animate();
        }
    }
}