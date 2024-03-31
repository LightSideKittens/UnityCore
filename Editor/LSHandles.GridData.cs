using System;
using Sirenix.Utilities;
using UnityEngine;

namespace LSCore.Editor
{
    public static partial class LSHandles
    {
        [Serializable]
        public class GridData
        {
            public int cellDivides = 10;
            public Vector2 scale = Vector2.one;
            public Color color = new (0.5f, 0.5f, 0.5f, 1);
            public Vector2 scaleRange = new(0.0001f, 100);
            public Vector2 stepRef = new Vector2(0.1f, 0.1f);

            public void Draw()
            {
                HandleGridInput();
                if(eventType != EventType.Repaint) return; 
                Vector2 startPoint = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
                Vector2 endPoint = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));
                var zoomStepMultiplies = GetGridMultiplyByZoom();
                var scaleStepMultiplies = GetGridMultiplyByGridScale();
                scaleStepMultiplies.x += zoomStepMultiplies;
                scaleStepMultiplies.y += zoomStepMultiplies;
                scaleStepMultiplies -= Vector2.one;

                var lw = 0.001f * cam.orthographicSize;
                var step = stepRef * scale;
                step *= scaleStepMultiplies;

                startPoint.x -= startPoint.x % step.x;
                startPoint.y -= startPoint.y % step.y;

                var positions = new Vector3[2];
                var c = color;

                var alphaByZoom = GetProgressToNextDoublingByZoom(1);
                var alphaByScale = GetProgressToNextDoublingByScale(1);
                alphaByScale *= alphaByZoom;

                float minValue = -100_000_000_000_000f;
                float maxValue = 100_000_000_000_000f;

                var index = Mathf.RoundToInt(startPoint.x / step.x);

                while (startPoint.x < endPoint.x)
                {
                    HandleOpacity(0);
                    var line = GetLine(c, lw);
                    positions[0] = new Vector3(startPoint.x, minValue);
                    positions[1] = new Vector3(startPoint.x, maxValue);
                    line.positionCount = 2;
                    line.SetPositions(positions);
                    startPoint.x += step.x;
                }
                
                index = Mathf.RoundToInt(startPoint.y / step.y);

                while (startPoint.y < endPoint.y)
                {
                    HandleOpacity(1);
                    var line = GetLine(c, lw);
                    positions[0] = new Vector3(minValue, startPoint.y);
                    positions[1] = new Vector3(maxValue, startPoint.y);
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
                        c.a = Mathf.Pow(alphaByZoom, 4);
                    }

                    index++;
                }
            }

            private void HandleGridInput()
            {
                Event e = Event.current;
                Vector3 mp = e.mousePosition;
                mp.y *= -1;
                mp.y += rect.height;
                var gridDelta = e.delta.y / 300;
            
                if (e.type == EventType.ScrollWheel)
                {
                    if (e.control)
                    {
                        scale.x -= gridDelta;
                        ClampGridScale();
                    }
                    else if(e.alt)
                    {
                        scale.y -= gridDelta;
                        ClampGridScale();
                    }
                    GUI.changed = true;
                }
            }
            
            private void ClampGridScale()
            {
                scale = scale.Clamp(Vector2.one * scaleRange.x, Vector2.one * scaleRange.y);
            }

            public float GetProgressToNextDoublingByZoom(int doublingDepth)
            {
                return GetLerped(camData.size, cellDivides, doublingDepth);
            }
            
            public float GetGridMultiplyByZoom()
            {
                float scaleRatio = camData.size;
                int doublingCount = (int)Mathf.Log(scaleRatio, cellDivides);
                float gridSpacingMultiply = Mathf.Pow(cellDivides, doublingCount);
                return gridSpacingMultiply;
            }
    
            public Vector2 GetGridMultiplyByGridScale()
            {
                int doublingXCount = (int)Mathf.Log(scale.x, 2);
                int doublingYCount = (int)Mathf.Log(scale.y, 2);
                float gridSpacingMultiplyX = Mathf.Pow(2, doublingXCount);
                float gridSpacingMultiplyY = Mathf.Pow(2, doublingYCount);

                return new Vector2(gridSpacingMultiplyX, gridSpacingMultiplyY);
            }
    
            public Vector2 GetProgressToNextDoublingByScale(int doublingDepth)
            {
                return new Vector2(
                    GetLerped(scale.x, 2, doublingDepth),
                    GetLerped(scale.y, 2, doublingDepth));
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