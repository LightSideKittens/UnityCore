using LSCore.Extensions.Unity;
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
        private Vector3 lastPosition;
        private Quaternion lastRotation;
        private Vector3 lastScale;
        
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
                DrivenTransformProperties.SizeDelta |
                DrivenTransformProperties.AnchorMin |
                DrivenTransformProperties.AnchorMax |
                DrivenTransformProperties.Pivot |
                DrivenTransformProperties.AnchoredPosition3D |
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
#if UNITY_EDITOR
            if (World.IsEditMode)
            {
                if(target == null || source == null) return;
            }
#endif
            target.gameObject.SetActive(gameObject.activeSelf);
            target.pivot = source.pivot;
            target.anchorMin = LSVector2.half;
            target.anchorMax = LSVector2.half;
            target.sizeDelta = source.rect.size;
            target.rotation = source.rotation;
            target.position = source.position;
            target.SetLossyScale(source.lossyScale);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            SyncTransforms();
        }
#endif
        
        private void Update()
        {
            var pos = source.position;
            if (lastPosition != pos)
            {
                target.position = pos;
                lastPosition = pos;
            }

            var rot = source.rotation;
            if (lastRotation != rot)
            {
                target.rotation = rot;
                lastRotation = rot;
            }
            
            var scale = source.lossyScale;
            if (lastScale != scale)
            {
                target.SetLossyScale(scale);
                lastScale = scale;
            }
        }
        
        protected override void OnRectTransformDimensionsChange()
        {
            SyncTransforms();
        }
    }
}