using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LSCore.Runtime
{
    public class NativeText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;

        private void Start()
        {
            text.ForceMeshUpdate();
            var texture = TextRenderer.RenderText(text);
            var rawImage = new GameObject("image").AddComponent<RawImage>();
            rawImage.texture = texture;
            rawImage.transform.SetParent(text.transform.parent);
            var rect = (RectTransform)rawImage.transform;
            CopyRectTransformValues(text.rectTransform, rect);
            /*Debug.Log($"Size: {rect.sizeDelta}");
            Debug.Log($"Size: {text.textBounds.size}");
            rect.sizeDelta = text.textBounds.size;*/
            text.enabled = false;
        }
        
        public static void CopyRectTransformValues(RectTransform source, RectTransform destination)
        {
            if (source == null || destination == null)
            {
                Debug.LogError("Source or destination RectTransform is null.");
                return;
            }

            destination.anchoredPosition = source.anchoredPosition;
            destination.sizeDelta = source.sizeDelta;
            destination.anchorMin = source.anchorMin;
            destination.anchorMax = source.anchorMax;
            destination.pivot = source.pivot;
            destination.localScale = source.localScale;
            destination.localRotation = source.localRotation;
            destination.localPosition = source.localPosition;
        }
    }
}