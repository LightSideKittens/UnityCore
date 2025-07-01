using LSCore.Extensions.Unity;
using UnityEngine;

namespace LSCore.UIModule
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public sealed class RootCanvasFitter : MonoBehaviour
    {
        private RectTransform self;
        private RectTransform root;
        private DrivenRectTransformTracker tracker;
        
        private void Awake() => CacheReferencesAndFit();
        private void OnEnable() => CacheReferencesAndFit();

#if UNITY_EDITOR
        private void OnValidate() => CacheReferencesAndFit();
#endif

        private bool ignoreFitRoot;
        private void OnTransformParentChanged() => CacheReferencesAndFit();
        private void OnRectTransformDimensionsChange()
        {
            if(ignoreFitRoot) return;
            ignoreFitRoot = true;
            FitToRoot();
            ignoreFitRoot = false;
        }
        
        private void CacheReferencesAndFit()
        {
            if (!self) self = (RectTransform)transform;

            var canvas = GetComponentInParent<Canvas>();
            root = canvas ? canvas.rootCanvas.GetComponent<RectTransform>() : null;

            FitToRoot();
        }

        private void FitToRoot()
        {
            if (!self || !root) return;
            
            tracker.Clear(); 
            tracker.Add(this, self,
                DrivenTransformProperties.AnchorMin |
                DrivenTransformProperties.AnchorMax |
                DrivenTransformProperties.AnchoredPosition |
                DrivenTransformProperties.SizeDelta |
                DrivenTransformProperties.Pivot);

            self.pivot = LSVector2.half;
            self.anchorMin = Vector2.zero;
            self.anchorMax = Vector2.one;
            self.offsetMin = Vector2.zero;
            self.offsetMax = Vector2.zero;

            var rootRect = root.rect;
            var selfRect = self.rect;
            var dif = rootRect.size - selfRect.size;
            self.sizeDelta = dif;
            self.position = root.position;
        }
    }
}