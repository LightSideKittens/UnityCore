using System;
using System.Globalization;
using LSCore.Extensions.Unity;
using Sirenix.Utilities;
using UnityEngine;

namespace LSCore.Editor
{
    public static partial class LSHandles
    {
        public static Vector3 Scale => Vector3.one.Divide(currentMatrix.lossyScale) * camData.Size;
        
        [Serializable]
        public class GridData
        {
            public int cellDivides = 10;
            public Color color = new(0, 0, 0, 1f);
            public Vector2 scaleRange = new(0.0001f, 100);
            public Vector2 stepRef = new Vector2(0.1f, 0.1f);
            public bool displayScale = true;
            public Color scaleBackColor = new Color(0f, 0f, 0f, 0.5f);
            
            public void Draw()
            {
                if (eventType != EventType.Repaint) return;

                Vector2 minCamPos = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
                Vector2 maxCamPos = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));

                using (SetMatrix(Matrix4x4.identity))
                {
                    var rect = SelectRect.CreateRect(minCamPos, maxCamPos);
                    var top = rect.TakeFromBottom(0.3f);
                    top = top.AlignCenter(top.width / ScaleMultiplier * 1.1f);
                    DrawSquare(top, scaleBackColor, false);
                }
                
                Vector2 startPoint = minCamPos;
                Vector2 endPoint = maxCamPos;
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
                var textOffsetX = 0.02f * cam.orthographicSize * ScaleMultiplier;
                var textOffsetY = 0.03f * cam.orthographicSize * ScaleMultiplier;
                
                double startX = startPoint.x;
                double endX = endPoint.x;
                
                double startY = startPoint.y;
                double endY = endPoint.y;
                
                double stepX = step.x;
                double stepY = step.y;
                
                var index = Mathf.RoundToInt((float)(startX / stepX));
                
                while (startX < endX)
                {
                    HandleOpacity(0);
                    var line = GetLine(c, lw, false);

                    if (displayScale && (index - 1) % cellDivides == 0)
                    {
                        var tx = (float)startX;
                        var tpos = currentMatrix.MultiplyPoint3x4(new Vector3(tx, 0, 0));
                        tpos.y = maxCamPos.y - textOffsetY;
                        var text = DrawText(FormatFloatSmart(tx), tpos, TextAnchor.MiddleCenter);
                    }
                    
                    positions[0] = currentMatrix.MultiplyPoint3x4(new Vector3((float)startX, 0)).Y(minValue);
                    positions[1] = currentMatrix.MultiplyPoint3x4(new Vector3((float)startX, 0)).Y(maxValue);

                    line.positionCount = 2;
                    line.SetPositions(positions);
                    startX += stepX;
                }

                index = Mathf.RoundToInt((float)(startY / stepY));

                while (startY < endY)
                {
                    HandleOpacity(1);
                    var line = GetLine(c, lw, false);
                    
                    if (displayScale && (index - 1) % cellDivides == 0)
                    {
                        var ty = (float)startY;
                        var tpos = currentMatrix.MultiplyPoint3x4(new Vector3(0, ty, 0));
                        tpos.x = minCamPos.x + textOffsetX;
                        var text = DrawText(FormatFloatSmart(ty), tpos, TextAnchor.MiddleLeft);
                    }
                    
                    positions[0] = currentMatrix.MultiplyPoint3x4(new Vector3(0, (float)startY)).X(minValue);
                    positions[1] = currentMatrix.MultiplyPoint3x4(new Vector3(0, (float)startY)).X(maxValue);

                    line.positionCount = 2;
                    line.SetPositions(positions);
                    startY += stepY;
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

                TextMesh DrawText(string message, Vector3 pos, TextAnchor anchor)
                {
                    var text = Canvas.GetText(message, 32);
                    text.anchor = anchor;
                    pos.z = -1;
                    text.transform.position = pos;
                     
                    return text;
                }
            }
            
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
            
            public static string FormatFloatSmart(float value)
            {
                double dbl = value;

                dbl = Math.Round(dbl, 6, MidpointRounding.AwayFromZero);

                double nearestInt = Math.Round(dbl, 0, MidpointRounding.AwayFromZero);
                if (Math.Abs(dbl - nearestInt) < 1e-6)
                {
                    return nearestInt.ToString("0", CultureInfo.InvariantCulture);
                }

                string str = dbl.ToString("0.######", CultureInfo.InvariantCulture);

                if (str.Contains("."))
                {
                    str = str.TrimEnd('0').TrimEnd('.');
                }

                return str;
            }
        }
    }
}
