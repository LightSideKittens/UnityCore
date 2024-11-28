using UnityEngine;
using UnityEngine.EventSystems;

namespace LSCore
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class RectTransformSyncer : UIBehaviour
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private Vector2 anchoredPosOffset;

        private RectTransform source;
        private DrivenRectTransformTracker tracker;
        private Vector2 lastPosition;
        
        protected override void Awake()
        {
            source = GetComponent<RectTransform>();
        }

        protected override void OnEnable()
        {
            StartSync();
        }

        protected override void OnDisable()
        {
            StopSync();
        }

        private void StartSync()
        {
            if (target == null || source == null)
                return;
            
            tracker.Clear();
            tracker.Add(this, target, 
                DrivenTransformProperties.AnchoredPosition |
                DrivenTransformProperties.SizeDelta |
                DrivenTransformProperties.AnchorMin |
                DrivenTransformProperties.AnchorMax |
                DrivenTransformProperties.Pivot |
                DrivenTransformProperties.Rotation |
                DrivenTransformProperties.Scale);
            
            SyncTransforms();
        }

        private void StopSync()
        {
            tracker.Clear();
        }

        private void SyncTransforms()
        {
            if (target == null || source == null)
                return;

            target.gameObject.SetActive(gameObject.activeSelf);
            target.anchoredPosition = source.anchoredPosition + anchoredPosOffset;
            target.sizeDelta = source.sizeDelta;
            target.anchorMin = source.anchorMin;
            target.anchorMax = source.anchorMax;
            target.pivot = source.pivot;
            target.rotation = source.rotation;
            target.localScale = source.localScale;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            SyncTransforms();
        }
#endif
        
        private void Update()
        {
#if UNITY_EDITOR
            if (World.IsEditMode)
            {
                if(target == null) return;
            }
#endif
            var srcPos = source.anchoredPosition;
            if (srcPos != lastPosition)
            {
                target.anchoredPosition = srcPos + anchoredPosOffset;
                lastPosition = srcPos;
            }
        }
        
        protected override void OnRectTransformDimensionsChange()
        {
            SyncTransforms();
        }
    }
}