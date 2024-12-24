using System;
using LSCore.Extensions.Unity;
using UnityEngine;

namespace LSCore.Editor
{
    public static partial class LSHandles
    {
        [Serializable]
        public class GridData
        {
            public int cellDivides = 10;
            public Color color = new(0.5f, 0.5f, 0.5f, 0.5f);
            public Vector2 scaleRange = new(0.0001f, 100);
            public Vector2 stepRef = new Vector2(0.1f, 0.1f);

            public void Draw()
            {
                if (eventType != EventType.Repaint) return;

                Vector2 startPoint = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
                Vector2 endPoint = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));
                var invMatrix = currentMatrix.inverse;
                startPoint = invMatrix.MultiplyPoint3x4(startPoint);
                endPoint = invMatrix.MultiplyPoint3x4(endPoint);
                
                Vector2 step = stepRef ;
                var scaleStepMultiplies = GetGridMultiply();
                step *= scaleStepMultiplies;
                
                startPoint.x -= startPoint.x % step.x;
                startPoint.y -= startPoint.y % step.y;

                var positions = new Vector3[2];
                var c = color;

                var alphaByZoom = GetProgressToNextDoubling();
                
                float minValue = -100_000_000_000_000f;
                float maxValue = 100_000_000_000_000f;

                var lw = 0.001f * cam.orthographicSize;
                var index = Mathf.RoundToInt(startPoint.x / step.x);
                
                while (startPoint.x < endPoint.x)
                {
                    HandleOpacity(0);
                    var line = GetLine(c, lw, false);

                    positions[0] = currentMatrix.MultiplyPoint3x4(new Vector3(startPoint.x, minValue));
                    positions[1] = currentMatrix.MultiplyPoint3x4(new Vector3(startPoint.x, maxValue));

                    line.positionCount = 2;
                    line.SetPositions(positions);
                    startPoint.x += step.x;
                }

                index = Mathf.RoundToInt(startPoint.y / step.y);

                while (startPoint.y < endPoint.y)
                {
                    HandleOpacity(1);
                    var line = GetLine(c, lw, false);

                    positions[0] = currentMatrix.MultiplyPoint3x4(new Vector3(minValue, startPoint.y));
                    positions[1] = currentMatrix.MultiplyPoint3x4(new Vector3(maxValue, startPoint.y));

                    line.positionCount = 2;
                    line.SetPositions(positions);
                    startPoint.y += step.y;
                }

                void HandleOpacity(int compIndex)
                {
                    if (index % cellDivides == 0)
                    {
                        lw = 0.002f * cam.orthographicSize;
                        c.a = color.a;
                    }
                    else
                    {
                        lw = 0.002f * cam.orthographicSize;
                        c.a = Mathf.Pow(alphaByZoom[compIndex], 4);
                    }

                    index++;
                }
            }

            
            private Vector3 Scale => Vector3.one.Divide(currentMatrix.lossyScale) * camData.Size;
            public Vector2 GetGridMultiply()
            {
                return NextPowerOfScale(Scale, cellDivides);
            }

            private static Vector2 NextPowerOfScale(Vector3 scale, int cellDivides)
            {
                var min = Math.Pow(cellDivides, -5);
                var max = Mathf.Pow(cellDivides, 6);
                return new Vector2(NextPowerOf((int)(scale.x / min), cellDivides) / max, NextPowerOf((int)(scale.y / min), cellDivides) / max);
            }

            private static int NextPowerOf(int value, int baseValue)
            {
                if (value < 1)
                    return 1;

                float logValue = Mathf.Log(value, baseValue);
                int ceilPower = Mathf.CeilToInt(logValue);
                int candidate = (int)Mathf.Pow(baseValue, ceilPower);
                if (candidate <= value)
                {
                    candidate *= baseValue;
                }

                return candidate;
            }

            public Vector2 GetProgressToNextDoubling()
            {
                var scale = Scale;
                var current = NextPowerOfScale(scale, cellDivides);

                var prev = current / cellDivides;
                var next = current * cellDivides;
                
                return new Vector2(Mathf.InverseLerp(next.x, prev.x, scale.x), Mathf.InverseLerp(next.y, prev.y, scale.y));
            }

            public static float GetLerped(float scaleRatio, float logBase, int depth)
            {
                float value = Mathf.Log(scaleRatio, logBase);
                float a = (int)value;
                float b = a + depth;

                return Mathf.InverseLerp(b, a, value);
            }
        }
    }
}
