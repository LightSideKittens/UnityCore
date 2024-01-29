using System;
using UnityEngine;
using static Animatable.AnimatableCanvas;
using Object = UnityEngine.Object;

namespace Animatable
{
    [Serializable]
    public class HealthBar
    {
        [SerializeField] public LSCore.HealthBar mainBar;
        private Transform target;
        private Vector3 offset;

        public void Reset()
        {
            mainBar.gameObject.SetActive(true);
            mainBar.value = mainBar.maxValue;
        }
        
        public static HealthBar Create(float maxValue, Transform target, Vector2 offset, Vector2 scale, bool isOpponent)
        {
            var template = isOpponent ? OpponentHealthBar : AnimatableCanvas.HealthBar;
            
            var bar = Object.Instantiate(template.mainBar);
            var barTransform = bar.transform;
            barTransform.SetParent(SpawnPoint, false);
            barTransform.localScale = scale;
            bar.maxValue = maxValue;
            bar.value = maxValue;
            
            var result = new HealthBar
            {
                target = target,
                offset = offset,
                mainBar = bar,
            };

            return result;
        }

        public void Disable()
        { 
            mainBar.gameObject.SetActive(false);
        }

        public void Update()
        {
            mainBar.transform.localPosition = GetLocalPosition(target.position + offset);
        }

        public void SetValue(float value)
        {
            mainBar.value = value;
        }
    }
}