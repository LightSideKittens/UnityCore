using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class LSGradient
    {
        [SerializeField] private List<Color> colors = new();
        [SerializeField] private List<float> positions = new();

        public Color Evaluate(float time)
        {
            if (colors.Count == 0)
            {
                return Color.white;
            }

            if (colors.Count == 1)
            {
                return colors[0];
            }

            int start;
            int end = positions.Count - 1;

            for (start = 0; start < end; start++)
            {
                if (time >= positions[start] && time <= positions[start + 1])
                {
                    end = start + 1;
                    break;
                }
            }

            var startPos = positions[start];
            var endPos = positions[start];
            float blendFactor = (time - startPos) / (endPos - startPos);
            return Color.Lerp(colors[start], colors[end], blendFactor);
        }
    }
}