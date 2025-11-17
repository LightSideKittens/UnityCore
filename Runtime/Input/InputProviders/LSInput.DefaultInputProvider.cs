using LSCore.DataStructs;
using UnityEngine;

namespace LSCore
{
    public static partial class LSInput
    {
        internal class DefaultInputProvider : IInputProvider
        {
            internal static DefaultInputProvider Instance { get; set; } = new();

            private Vector2 lastPosition;
            private bool isTouching;

            internal DefaultInputProvider()
            {
            }

            public ArraySlice<LSTouch> GetTouches()
            {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
                var count = Input.touchCount;
                
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        var tt = Input.GetTouch(i);
                        touchesBuffer[i] = new LSTouch
                        {
                            fingerId = tt.fingerId,
                            position = tt.position,
                            deltaPosition = tt.deltaPosition,
                            phase = tt.phase
                        };
                    }
                    return touchesBuffer.Slice(..count);
                }
                
                return ArraySlice<LSTouch>.empty;
#endif
                
                Vector2 current = Input.mousePosition;
                LSTouch? t = null;

                if (Input.GetMouseButtonDown(0))
                {
                    isTouching = true;
                    lastPosition = current;
                    t = new LSTouch
                        { fingerId = -1, position = current, deltaPosition = Vector2.zero, phase = TouchPhase.Began };
                }
                else if (Input.GetMouseButton(0))
                {
                    var delta = current - lastPosition;
                    var phase = delta.sqrMagnitude > 0f ? TouchPhase.Moved : TouchPhase.Stationary;
                    t = new LSTouch { fingerId = -1, position = current, deltaPosition = delta, phase = phase };
                    lastPosition = current;
                }
                else if (isTouching && Input.GetMouseButtonUp(0))
                {
                    isTouching = false;
                    t = new LSTouch
                        { fingerId = -1, position = current, deltaPosition = Vector2.zero, phase = TouchPhase.Ended };
                }

                if (t != null)
                {
                    touchesBuffer[0] = t.Value;
                    return touchesBuffer.Slice(..1);
                }

                return ArraySlice<LSTouch>.empty;
            }
        }
    }
}