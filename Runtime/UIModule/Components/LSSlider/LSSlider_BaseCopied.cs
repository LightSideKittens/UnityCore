using UnityEngine;
using UnityEngine.UI;

namespace LSCore
{
    public partial class LSSlider
    {
        private float ClampValue(float input)
        {
            float newValue = Mathf.Clamp(input, minValue, maxValue);
            if (wholeNumbers)
                newValue = Mathf.Round(newValue);
            return newValue;
        }

        private Image fillImage;
        private Transform fillTransform;
        private Transform fillContainerRect;
        private Transform handleTransform;
        private Transform handleContainerRect;

        private void UpdateCachedReferences()
        {
            if (fillRect && fillRect != (RectTransform)transform)
            {
                fillTransform = fillRect.transform;
                fillImage = fillRect.GetComponent<Image>();
                if (fillTransform.parent != null)
                    fillContainerRect = fillTransform.parent.GetComponent<RectTransform>();
            }
            else
            {
                fillRect = null;
                fillContainerRect = null;
                fillImage = null;
            }

            if (handleRect && handleRect != (RectTransform)transform)
            {
                handleTransform = handleRect.transform;
                if (handleTransform.parent != null)
                    handleContainerRect = handleTransform.parent.GetComponent<RectTransform>();
            }
            else
            {
                handleRect = null;
                handleContainerRect = null;
            }
        }

        private enum Axis
        {
            Horizontal = 0,
            Vertical = 1
        }

        private bool reverseValue
        {
            get { return direction == Direction.RightToLeft || direction == Direction.TopToBottom; }
        }

        private Axis axis
        {
            get
            {
                return (direction == Direction.LeftToRight || direction == Direction.RightToLeft)
                    ? Axis.Horizontal
                    : Axis.Vertical;
            }
        }

        private DrivenRectTransformTracker m_Tracker;

        private void UpdateVisuals()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                UpdateCachedReferences();
#endif

            m_Tracker.Clear();

            if (fillContainerRect != null)
            {
                m_Tracker.Add(this, fillRect, DrivenTransformProperties.Anchors);
                Vector2 anchorMin = Vector2.zero;
                Vector2 anchorMax = Vector2.one;

                if (fillImage != null && fillImage.type == Image.Type.Filled)
                {
                    fillImage.fillAmount = normalizedValue;
                }
                else
                {
                    if (reverseValue)
                        anchorMin[(int)axis] = 1 - normalizedValue;
                    else
                        anchorMax[(int)axis] = normalizedValue;
                }

                fillRect.anchorMin = anchorMin;
                fillRect.anchorMax = anchorMax;
            }

            if (handleContainerRect != null)
            {
                m_Tracker.Add(this, handleRect, DrivenTransformProperties.Anchors);
                Vector2 anchorMin = Vector2.zero;
                Vector2 anchorMax = Vector2.one;
                anchorMin[(int)axis] = anchorMax[(int)axis] = (reverseValue ? (1 - normalizedValue) : normalizedValue);
                handleRect.anchorMin = anchorMin;
                handleRect.anchorMax = anchorMax;
            }
        }
    }
}