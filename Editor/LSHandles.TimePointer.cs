using System;
using Sirenix.Utilities;
using UnityEngine;

namespace LSCore.Editor
{
    public static partial class LSHandles
    {
        [Serializable]
        public class TimePointer
        {
            public Color color = new(0.09f, 0.67f, 0.96f);
            [SerializeField] private float time;
            
            public float Time
            {
                get => time;
                set
                {
                    time = value;
                    
                    if (loop)
                    {
                        var diff = time - lastTime;
                        if (diff > 0 && time >= clampRange.y)
                        {
                            time = clampRange.x;
                        }
                        else if(diff < 0 && time <= clampRange.x)
                        {
                            time = clampRange.y;
                        }
                    }
                    else
                    {
                        time = Mathf.Clamp(time, clampRange.x, clampRange.y);
                    }

                    lastTime = time;
                }
            }
            
            public Vector2 clampRange = new (0, float.MaxValue);
            public bool loop = true;
            private float lastTime;
            private bool isClicked;

            public void OnGUI()
            {
                Vector2 minCamPos = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
                Vector2 maxCamPos = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));

                var rect = SelectRect.CreateRect(minCamPos, maxCamPos);
                var top = rect.TakeFromBottom(0.3f);
                var pointerLineRect = rect.AlignCenter(0.03f);
                top = top.AlignCenter(top.width / ScaleMultiplier * 1.1f);
                var mouseClickArea = top;

                if (eventType == EventType.Repaint)
                {
                    var nextPowerOfScale = FloatFormatter.NextPowerOfScale(Scale, 10);
                    var text = FloatFormatter.Format(nextPowerOfScale.x / 10, Time);
                    
                    top = top.AlignCenter(0.1f * (text.Length + 1), 0.25f);
                    top.x -= camData.position.x;
                    top.x += currentMatrix.MultiplyPoint3x4(new Vector3(Time, 0, 0)).x;
                    pointerLineRect = pointerLineRect.SetCenterX(top.center.x);

                    using (SetMatrix(Matrix4x4.identity))
                    {
                        DrawSquare(top, color, false);
                        DrawSquare(pointerLineRect, color, false);
                        DrawText(text.ToBold(), top.center, TextAnchor.MiddleCenter);
                    }
                }

                var e = Event.current;

                if (e.type == EventType.MouseDown)
                {
                    var mp = MouseInWorldPoint;
                    using (SetMatrix(Matrix4x4.identity))
                    {
                        if (mouseClickArea.Contains(MouseInWorldPoint))
                        {
                            Time = mp.x;
                            isClicked = true;
                            e.Use();
                        }
                    }
                }
                else if (e.type == EventType.MouseDrag)
                {
                    var mp = MouseInWorldPoint;
                    using (SetMatrix(Matrix4x4.identity))
                    {
                        if (isClicked)
                        {
                            Time = mp.x;
                            e.Use();
                        }
                    }
                }
                else if (e.type == EventType.MouseUp)
                {
                    if (isClicked)
                    {
                        e.Use();
                    }
                    
                    isClicked = false;
                }

                TextMesh DrawText(string message, Vector3 pos, TextAnchor anchor)
                {
                    var text = Canvas.GetText(message, 30);
                    text.anchor = anchor;
                    pos.z = -1;
                    text.transform.position = pos;

                    return text;
                }
            }
        }
    }
}