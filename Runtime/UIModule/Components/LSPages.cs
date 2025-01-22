using System.Collections.Generic;
using DG.Tweening;
using LSCore.AnimationsModule;
using LSCore.AnimationsModule.Animations;
using LSCore.AnimationsModule.Animations.Options;
using LSCore.Async;
using UnityEngine;

namespace LSCore
{
    public class LSPages : MonoBehaviour, ISerializationCallbackReceiver
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private GameObject pagesSwitcher;
        [SerializeField] private CustomContentSizeFitter content;
        [SerializeField] private LSButton nextPage;
        [SerializeField] private LSButton previousPage;
        [SerializeField] private LSText pagesCounter;
        [SerializeField] private AnimSequencer anim;
        
        private int currentPage;
        private PivotPosXAnim pivotPosXAnim;
        private Tween currentTween;
        private Vector2 previousContentSize;
        
        private int TotalPages => (int)(pivotPosXAnim.target.sizeDelta.x / rectTransform.sizeDelta.x);
        
        private void OnEnable()
        {
            Wait.Frames(1, () =>
            {
                SetPage(0);
                content.SizeChanged += OnContentSizeChanged;
            });

            nextPage.Clicked += NextPage;
            previousPage.Clicked += PreviousPage;
        }

        private void OnDisable()
        {
            SetPage(0);
            
            nextPage.Clicked -= NextPage;
            previousPage.Clicked -= PreviousPage;
            content.SizeChanged -= OnContentSizeChanged;
        }

        private void NextPage()
        {
            SetPage(currentPage + 1);
        }

        private void PreviousPage()
        {
            SetPage(currentPage - 1);
        }

        private void OnContentSizeChanged(Vector2 size)
        {
            if(size == previousContentSize) return;
            
            previousContentSize = size;
            SetPage(currentPage >= TotalPages ? TotalPages - 1 : currentPage, false);
        }
        
        private void SetPage(int page, bool animate = true)
        {
            pagesSwitcher.SetActive(TotalPages > 1);
            if (TotalPages < 1) return;

            if (animate)
            {
                currentTween?.Kill();
                pivotPosXAnim.endValue = (float) page / TotalPages;
                currentTween = anim.Animate();
            }
            else
            {
                var pivot = pivotPosXAnim.target.pivot;
                pivot.x = (float) page / TotalPages;
                pivotPosXAnim.target.pivot = pivot;
            }
            
            pagesCounter.text = $"{page + 1}/{TotalPages}";
            previousPage.enabled = page > 0;
            nextPage.enabled = page < TotalPages - 1;
            
            currentPage = page;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if(World.IsEditMode) return;
            
            if (!anim.Contains<PivotPosXAnim>())
            {
                pivotPosXAnim = new PivotPosXAnim {Duration = 0.3f, target = content.rectTransform, options = new List<IOption> {new SetEase {ease = Ease.OutBack}}};
                anim.Add(new AnimSequencer.AnimData { timeOffset = 0, anim = pivotPosXAnim });
            }
            else
            {
                pivotPosXAnim = anim.GetAnim<PivotPosXAnim>();
            }
        }
    }
}