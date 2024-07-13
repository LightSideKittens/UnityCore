using System;
using System.Threading.Tasks;
using LSCore.Extensions.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LSCore.Runtime
{
    public class NativeText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;

        private async void Start()
        {
            Debug.Log(DateTime.Now);
            await Task.Delay(10000);
            Debug.Log(DateTime.Now);
            text.ForceMeshUpdate();
            var texture = TextRenderer.RenderText(text);
            var imageGo = new GameObject("image");
            var rawImage = imageGo.AddComponent<RawImage>();
            rawImage.texture = texture;
            rawImage.transform.SetParent(text.transform.parent);
            
            var fitter = imageGo.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
            fitter.aspectRatio = texture.AspectRatio();
            var pivot = text.rectTransform.pivot;
            switch (text.verticalAlignment)
            {
                case VerticalAlignmentOptions.Top:
                    pivot.y = 1;
                    break;
                case VerticalAlignmentOptions.Middle:
                    pivot.y = 0.5f;
                    break;
                case VerticalAlignmentOptions.Bottom:
                    pivot.y = 0;
                    break;
            }

            rawImage.rectTransform.anchoredPosition = text.rectTransform.anchoredPosition;
            rawImage.rectTransform.SetSizeDeltaX(text.rectTransform.sizeDelta.x);
            rawImage.rectTransform.SetPivot(pivot);
            text.enabled = false;
        }
    }
}