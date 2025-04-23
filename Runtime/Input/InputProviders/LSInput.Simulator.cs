using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSCore
{
    public static partial class LSInput
    {
        public class Simulator : IInputProvider
        {
            internal static Simulator Instance { get; set; } = new();

            public class Touch
            {
                public Vector2 position;
                public bool pendingUp;
                internal int fingerId;
                internal Vector2 lastPosition;
                internal bool pendingDown;

                public void Release() => pendingUp = true;
                
                internal Touch()
                {
                }
            }

            private readonly List<Touch> touches = new();

            internal Simulator()
            {
            }

            public static Touch TouchDown(Vector3 pos)
            {
                var touches = Instance.touches;
                var id = touches.Count;
                
                var touch = new Touch
                {
                    fingerId = id,
                    position = pos,
                    lastPosition = pos,
                    pendingDown = true,
                    pendingUp = false
                };

                touches.Add(touch);
                return touch;
            }
            
            public static void TouchMove(int index, Vector2 pos)
            {
                Instance.touches[index].position = pos;
            }

            public static void TouchUp(int index)
            {
                Instance.touches[index].pendingUp = true;
            }

            private readonly Touch[] dataBuffer = new Touch[20];
            
            public LSTouch[] GetTouches()
            {
                if(touches.Count == 0) return Array.Empty<LSTouch>();
                
                int count = 0;
                int endedCount = 0;
                
                foreach (var data in touches)
                {
                    var id = data.fingerId;
                    LSTouch touch;

                    if (data.pendingDown)
                    {
                        touch = new LSTouch
                        {
                            fingerId = id,
                            position = data.position,
                            deltaPosition = Vector2.zero,
                            phase = TouchPhase.Began
                        };
                        data.pendingDown = false;
                    }
                    else if (data.pendingUp)
                    {
                        touch = new LSTouch
                        {
                            fingerId = id,
                            position = data.position,
                            deltaPosition = Vector2.zero,
                            phase = TouchPhase.Ended
                        };
                        data.pendingUp = false;
                        dataBuffer[endedCount++] = data;
                    }
                    else
                    {
                        var delta = data.position - data.lastPosition;
                        var phase = delta.sqrMagnitude > 0f ? TouchPhase.Moved : TouchPhase.Stationary;
                        touch = new LSTouch
                        {
                            fingerId = id,
                            position = data.position,
                            deltaPosition = delta,
                            phase = phase
                        };
                        data.lastPosition = data.position;
                    }
                    
                    touchesBuffer[count++] = touch;
                }

                if (endedCount > 0)
                {
                    for (int i = 0; i < endedCount; i++)
                    {
                        touches.Remove(dataBuffer[i]);
                    }
                }
                
                return touchesBuffer[..count];
            }
        }
    }
}