using System;
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
            public int textOnEveryLine = 2;
            public Color color = new(0, 0, 0, 1f);
            public Vector2 stepRef = new(0.1f, 0.1f);
            public bool displayXScale = true;
            public bool displayYScale = true;
            public bool displayXGrid = true;
            public bool displayYGrid = true;
            public Color scaleBackColor = new(0f, 0f, 0f, 0.5f);
            public float SnappingStep { get; private set; }
            
            public void Draw()
            {
                const float minValue = -100_000_000_000_000f;
                const float maxValue = 100_000_000_000_000f;
                
                if (eventType != EventType.Repaint) return;

                Vector2 minCamPos = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
                Vector2 maxCamPos = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));

                using (SetIdentityMatrix())
                {
                    var rect = SelectRect.CreateRect(minCamPos, maxCamPos);
                    var top = rect.TakeFromBottom(0.3f * ScaleMultiplier);
                    DrawSquare(top, scaleBackColor, false);
                }

                Vector2 startPoint = minCamPos;
                Vector2 endPoint = maxCamPos;
                var invMatrix = currentMatrix.inverse;
                startPoint = invMatrix.MultiplyPoint3x4(startPoint);
                endPoint = invMatrix.MultiplyPoint3x4(endPoint);

                Vector2 step = stepRef;
                var nextPowerOfScale = FloatFormatter.NextPowerOfScale(Scale, 10);
                var scaleStepMultiplies = GetGridMultiply();
                step *= scaleStepMultiplies;

                startPoint.x -= startPoint.x % step.x;
                startPoint.y -= startPoint.y % step.y;

                var positions = new Vector3[2];
                var c = color;

                var progressToNextDoubling = GetProgressToNextDoubling();
                
                float lw;
                var textOffset = new Vector2(-0.02f, 0.03f) * (cam.orthographicSize * ScaleMultiplier);

                double startX = startPoint.x;
                double endX = endPoint.x;

                double startY = startPoint.y;
                double endY = endPoint.y;

                double stepX = step.x;
                double stepY = step.y;
                SnappingStep = step.x;
                
                var index = Mathf.RoundToInt((float)(startX / stepX));

                if (displayXGrid)
                {
                    while (startX < endX)
                    {
                        var tx = (float)startX;
                        HandleOpacity(0);
                        DrawLine(tx, 0, 1);
                        if (displayXScale) DrawGridText(tx, 0, 1, maxCamPos, TextAnchor.MiddleCenter);
                        startX += stepX;
                    }
                }

                index = Mathf.RoundToInt((float)(startY / stepY));

                if (displayYGrid)
                {
                    while (startY < endY)
                    {
                        var ty = (float)startY;
                        HandleOpacity(1);
                        DrawLine(0, ty, 0);
                        if (displayYScale) DrawGridText(ty, 1, 0, minCamPos, TextAnchor.MiddleLeft);
                        startY += stepY;
                    }
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
                        c.a = Mathf.Pow(progressToNextDoubling[compIndex], 4);
                    }

                    index++;
                }

                void DrawGridText(float value, int f, int s, Vector2 camPos, TextAnchor anchor)
                {
                    var every = textOnEveryLine;
                    if (progressToNextDoubling[f] < 0.8f) every *= cellDivides;

                    if ((index - 1) % every == 0)
                    {
                        Vector3 tpos = default;
                        tpos[f] = value;
                        tpos = currentMatrix.MultiplyPoint3x4(tpos);
                        tpos[s] = camPos[s] - textOffset[s];
                        var text = DrawText(FormatFloatSmart(f, value), tpos, anchor);
                    }
                }

                void DrawLine(float x, float y, int compIndex)
                {
                    var line = GetLine(c, lw, false);
                    positions[0] = currentMatrix.MultiplyPoint3x4(new Vector3(x, y)).SetByIndex(minValue, compIndex);
                    positions[1] = currentMatrix.MultiplyPoint3x4(new Vector3(x, y)).SetByIndex(maxValue, compIndex);

                    line.positionCount = 2;
                    line.SetPositions(positions);
                }

                TextMesh DrawText(string message, Vector3 pos, TextAnchor anchor)
                {
                    var text = Canvas.GetText(message, 32);
                    text.anchor = anchor;
                    text.transform.position = pos;

                    return text;
                }

                string FormatFloatSmart(int compIndex, float value)
                {
                    var nextPower = nextPowerOfScale[compIndex] / 10;
                    return FloatFormatter.Format(nextPower, value);
                }
            }

            public Vector2 GetGridMultiply()
            {
                return FloatFormatter.NextPowerOfScale(Scale, cellDivides);
            }

            public Vector2 GetProgressToNextDoubling()
            {
                var scale = Scale;
                var current = FloatFormatter.NextPowerOfScale(Scale, cellDivides);

                var prev = current / cellDivides;
                var next = current * cellDivides;

                return new Vector2(Mathf.InverseLerp(next.x, prev.x, scale.x),
                    Mathf.InverseLerp(next.y, prev.y, scale.y));
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
