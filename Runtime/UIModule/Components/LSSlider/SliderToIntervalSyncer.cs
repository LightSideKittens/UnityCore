using UnityEngine;

namespace LSCore
{
    [ExecuteAlways]
    public class SliderToIntervalSyncer : MonoBehaviour
    {
        [SerializeField] private LSText currentIntervalText;
        [SerializeField] private LSSlider slider;
        [SerializeField] private IntervalSlider intervalSlider;

        private void Awake()
        {
            intervalSlider.onValueChanged.AddListener(OnValueChanged);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(intervalSlider == null) return;
            if(slider == null) return;
            
            intervalSlider.onValueChanged.RemoveListener(OnValueChanged);
            intervalSlider.onValueChanged.RemoveListener(OnValueChanged);
            intervalSlider.onValueChanged.AddListener(OnValueChanged);
            OnValueChanged(intervalSlider.value);
        }
#endif

        private void OnValueChanged(float value)
        {
            var data = intervalSlider.GetIntervalData(value);
            slider.minValue = data.from.value;
            slider.maxValue = data.to.value;
            slider.value = value;
            if (currentIntervalText != null)
            {
                currentIntervalText.text = $"{data.index + 1}";
            }
        }
    }
}