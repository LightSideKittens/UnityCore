using System;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace LSCore.Editor
{
    public static partial class LSHandles
    {
        [Serializable]
        public class TimePointer : ISerializationCallbackReceiver
        {
            public Color color = new(0.09f, 0.67f, 0.96f);
            [SerializeField] private float time;
            public float OldRealTime { get; private set; }
            public float RealTime { get; private set; }

            public void SyncRealTime()
            {
                OldRealTime = RealTime;
            }
            
            public float Time
            {
                get => time;
                set
                {
                    var newTime = value;
                    
                    OldRealTime = RealTime;
                    RealTime = value;
                    
                    var max = clampRange.y - clampRange.x;
                    
                    if (loop)
                    {
                        if (newTime > clampRange.y)
                        {
                            newTime = clampRange.x + (newTime - clampRange.y) % max;
                        }
                        else if(newTime < clampRange.x)
                        {
                            newTime = clampRange.y + (newTime - clampRange.x) % max;
                        }
                    }
                    else
                    {
                        newTime = Mathf.Clamp(newTime, clampRange.x, clampRange.y);
                    }
                    
                    newTime = SnapX(newTime, SnappingStep);
                    time = newTime;
                }
            }
            
            [SerializeField] private Vector2 clampRange = new (0, float.MaxValue);

            public Vector2 ClampRange
            {
                get => clampRange;
                set
                {
                    if (!isClampRangeOverride)
                    {
                        clampRange = value;

                        if (float.IsNegativeInfinity(clampRange.y))
                        {
                            clampRange.y = 1;
                        }
                    }
                }
            }

            public float SnappingStep { get; set; }

            public bool isClampRangeOverride;
            public bool loop = true;
            public float lastClickX;
            public Color pointerColor = new(0f, 0f, 0f, 0.5f);
            private bool isClicked;
            private double lastClickTime = -1;
            [NonSerialized] public Rect mouseClickArea;

            public void SetTimeAtLastPosition()
            {
                Time = lastClickX;
            }
            
            public bool ContainsClickArea(Vector2 mousePosition) => mouseClickArea.Contains(mousePosition);
            
            public void OnGUI()
            {
                Vector2 minCamPos = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
                Vector2 maxCamPos = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));

                var camViewRect = SelectRect.CreateRect(minCamPos, maxCamPos);
                
                var rect = camViewRect;
                var top = rect.TakeFromBottom(0.3f * ScaleMultiplier);
                var pointerLineRect = rect.AlignCenter(0.03f * ScaleMultiplier);
                mouseClickArea = top;

                if (eventType == EventType.Repaint)
                {
                    var nextPowerOfScale = FloatFormatter.NextPowerOfScale(Scale, 10);
                    var text = FloatFormatter.Format(nextPowerOfScale.x / 10, Time);
                    
                    top = top.AlignCenter(0.1f * (text.Length + 1) * ScaleMultiplier, 0.25f * ScaleMultiplier);
                    top.x -= camData.position.x;
                    top.x += currentMatrix.MultiplyPoint3x4(new Vector3(Time, 0, 0)).x;
                    pointerLineRect = pointerLineRect.SetCenterX(top.center.x);
                    
                    var leftBlackRect = camViewRect;
                    var rightBlackRect = camViewRect;

                    leftBlackRect.x = currentMatrix.MultiplyPoint3x4(new Vector3(clampRange.x - 20000f, 0, 0)).x;
                    rightBlackRect.x = currentMatrix.MultiplyPoint3x4(new Vector3(clampRange.y, 0, 0)).x;
                    
                    leftBlackRect.width = 20000f * currentMatrix.lossyScale.x;
                    rightBlackRect.width = leftBlackRect.width ;
                    
                    var pos = pointerLineRect.center;
                    pos.y = pointerLineRect.yMax;
                    pos.x = currentMatrix.MultiplyPoint3x4(new Vector3(lastClickX, 0, 0)).x;
                    
                    using (SetIdentityMatrix())
                    {
                        DrawTriangle(pos, 0.2f, pointerColor, false);
                        DrawSquare(leftBlackRect, new Color(0f, 0f, 0f, 0.39f), false);
                        DrawSquare(rightBlackRect, new Color(0f, 0f, 0f, 0.39f), false);
                        DrawSquare(top, color, false);
                        DrawSquare(pointerLineRect, color, false);
                        DrawText(text.ToBold(), top.center, TextAnchor.MiddleCenter);
                    }
                }
                
                var e = Event.current;

                if (e.type == EventType.MouseDown)
                {
                    var mp = MouseInWorldPoint;
                    using (SetIdentityMatrix())
                    {
                        if (mouseClickArea.Contains(MouseInWorldPoint))
                        {
                            if (EditorApplication.timeSinceStartup - lastClickTime < 0.3f)
                            {
                                isClampRangeOverride = false;
                            }
                            
                            lastClickTime = EditorApplication.timeSinceStartup;
                            isClicked = true;
                            
                            if (e.button == 0)
                            {
                                Time = mp.x;
                                lastClickX = Time;
                            }
                            else if (e.button == 1 && e.shift)
                            {
                                SetClampRange(mp.x);
                            }
                            else
                            {
                                isClicked = false;
                            }
                            
                            if(isClicked) e.Use();
                        }
                    }
                }
                else if (e.type == EventType.MouseDrag)
                {
                    var mp = MouseInWorldPoint;
                    using (SetIdentityMatrix())
                    {
                        if (isClicked)
                        {
                            if (e.button == 0)
                            {
                                Time = mp.x;
                                lastClickX = Time;
                            }
                            else if (e.button == 1 && e.shift)
                            {
                                SetClampRange(mp.x);
                            }
                            
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

                void SetClampRange(float x)
                {
                    isClampRangeOverride = true;
                    int compIndex = 0;
                    
                    if (Mathf.Abs(x - clampRange.x) > Mathf.Abs(x - clampRange.y))
                    {
                        compIndex = 1;
                    }
                    x = SnapX(x, SnappingStep);
                    clampRange[compIndex] = x;
                    
                    if (clampRange.x > clampRange.y)
                    {
                        (clampRange.x, clampRange.y) = (clampRange.y, clampRange.x);
                    }
                }
            }

            public void OnBeforeSerialize() { }

            public void OnAfterDeserialize()
            {
                RealTime = time;
            }
        }
    }
}